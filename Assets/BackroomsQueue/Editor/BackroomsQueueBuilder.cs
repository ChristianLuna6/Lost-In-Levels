// Backrooms - The Queue - Unity setup helper
// Builds materials (Built-in / URP / HDRP), prefabs (colliders + LOD groups) and the demo scene 'The Thousand Waits'
// from the FBX models, PNG textures and JSON files shipped with the pack.
//
// Expected layout (the kit folder can live anywhere under Assets; it is found from this script's location):
//   <Kit>/Models    <- contents of the pack's  fbx/  folder (keep sub-folders)
//   <Kit>/Textures  <- contents of the pack's  resources/textures/  folder
//   <Kit>/Layouts   <- contents of the pack's  unity/Layouts/  folder (scene layouts + Materials.json)
//   <Kit>/Editor    <- this file
// Then run:  Tools > Backrooms Queue > Build Everything
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BackroomsQueue.EditorTools
{
    public static class BackroomsQueueBuilder
    {
        static string s_Root;
        static string Root => s_Root ??= FindRoot();
        static string FindRoot()
        {
            var p = AssetDatabase.FindAssets("BackroomsQueueBuilder t:MonoScript").Select(AssetDatabase.GUIDToAssetPath)
                    .FirstOrDefault(x => Path.GetFileName(x) == "BackroomsQueueBuilder.cs");
            return p == null ? "Assets/BackroomsQueue" : Path.GetDirectoryName(Path.GetDirectoryName(p)).Replace('\\', '/');
        }
        static string TexDir => Root + "/Textures";
        static string ModelDir => Root + "/Models";
        static string LayoutDir => Root + "/Layouts";
        static string MatDir => Root + "/Materials";
        static string PrefabDir => Root + "/Prefabs";
        static string SceneDir => Root + "/Scenes";

        enum Pipeline { BuiltIn, URP, HDRP }

        [Serializable] public class MatDef { public string name; public string tex; public bool clip; public bool blend; public float emission;
            public float[] color; public float[] emissionColor; public float metallic; public float smoothness; public float[] tiling; }
        [Serializable] public class LodPair { public string lod0; public string lod1; }
        [Serializable] public class MatFile { public MatDef[] materials; public LodPair[] lods; }

        [MenuItem("Tools/Backrooms Queue/Build Everything", priority = 0)]
        public static void BuildAll()
        {
            if (!ReadyToModifyScenes()) return;
            try
            {
                EditorUtility.DisplayProgressBar("Backrooms Queue", "Configuring textures", 0.05f);
                ConfigureTextures();
                EditorUtility.DisplayProgressBar("Backrooms Queue", "Building materials", 0.2f);
                var mats = BuildMaterials();
                EditorUtility.DisplayProgressBar("Backrooms Queue", "Building prefabs", 0.4f);
                var prefabs = BuildPrefabs(mats);
                EditorUtility.DisplayProgressBar("Backrooms Queue", "Building demo scenes", 0.75f);
                BuildScenes(prefabs, mats);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Backrooms Queue", $"Done.\nMaterials: {mats.Count}\nPrefabs: {prefabs.Count}", "OK");
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        static bool s_ForceScenes;
        [MenuItem("Tools/Backrooms Queue/Rebuild Grouped Scenes (overwrite)", priority = 1)]
        public static void MenuRebuildScenes() { s_ForceScenes = true; try { BuildAll(); } finally { s_ForceScenes = false; } }

        [MenuItem("Tools/Backrooms Queue/1 - Configure Textures")] public static void MenuTextures() => ConfigureTextures();
        [MenuItem("Tools/Backrooms Queue/2 - Build Materials")] public static void MenuMaterials() => BuildMaterials();
        [MenuItem("Tools/Backrooms Queue/3 - Build Prefabs")] public static void MenuPrefabs() { if (ReadyToModifyScenes()) BuildPrefabs(BuildMaterials()); }
        [MenuItem("Tools/Backrooms Queue/4 - Build Demo Scenes")] public static void MenuScenes() { if (!ReadyToModifyScenes()) return; var m = BuildMaterials(); BuildScenes(BuildPrefabs(m), m); }

        static bool ReadyToModifyScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogError("[BackroomsQueue] Exit Play Mode first."); return false; }
            return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        static readonly string[] KitPrefixes = { "SM_QUE_" };
        static bool IsStructure(string n) => KitPrefixes.Any(p => n.StartsWith(p)) && (n.Contains("_Wall_") || n.Contains("_Floor_") || n.Contains("_Roof_") || n.Contains("_RoofTruss_") || n.Contains("_Column") || n.Contains("_Rail_") || n.Contains("_Flume_") || n.Contains("_Platform_") || n.Contains("_Station_Facade"));
        // decals, posters and loose paper: no collider, no shadows
        static bool IsFlat(string n) => n.Contains("_Decal_");

        static Pipeline CurrentPipeline()
        {
            var rp = GraphicsSettings.currentRenderPipeline;
            if (rp == null) return Pipeline.BuiltIn;
            return rp.GetType().Name.Contains("HDRenderPipeline") ? Pipeline.HDRP : Pipeline.URP;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static MatFile s_Mats;
        static MatFile Defs()
        {
            if (s_Mats != null) return s_Mats;
            var p = $"{LayoutDir}/Materials.json";
            if (!File.Exists(p)) { Debug.LogError($"[BackroomsQueue] Missing {p}"); return new MatFile { materials = new MatDef[0], lods = new LodPair[0] }; }
            s_Mats = JsonUtility.FromJson<MatFile>(File.ReadAllText(p));
            return s_Mats;
        }

        // ------------------------------------------------------------------ textures
        public static void ConfigureTextures()
        {
            if (!AssetDatabase.IsValidFolder(TexDir)) { Debug.LogError($"[BackroomsQueue] Missing folder {TexDir}"); return; }
            var alphaSets = new HashSet<string>(Defs().materials.Where(m => (m.clip || m.blend) && !string.IsNullOrEmpty(m.tex)).Select(m => m.tex));
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { TexDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;
                var n = Path.GetFileNameWithoutExtension(path);
                bool changed = false;
                if (n.EndsWith("_Normal"))
                {
                    if (ti.textureType != TextureImporterType.NormalMap) { ti.textureType = TextureImporterType.NormalMap; changed = true; }
                }
                else if (n.EndsWith("_ORM") || n.EndsWith("_MetallicSmoothness") || n.EndsWith("_MaskMap"))
                {
                    if (ti.sRGBTexture) { ti.sRGBTexture = false; changed = true; }
                }
                var set = n.EndsWith("_BaseColor") ? n.Substring(0, n.Length - "_BaseColor".Length) : null;
                if (set != null && alphaSets.Contains(set))
                {
                    if (!ti.alphaIsTransparency) { ti.alphaIsTransparency = true; changed = true; }
                    if (!ti.mipMapsPreserveCoverage) { ti.mipMapsPreserveCoverage = true; ti.alphaTestReferenceValue = 0.5f; changed = true; }
                }
                if (n.StartsWith("T_Atlas_") && ti.wrapMode != TextureWrapMode.Clamp) { ti.wrapMode = TextureWrapMode.Clamp; changed = true; }
                if (changed) ti.SaveAndReimport();
            }
        }

        static Texture2D Tex(string name) => string.IsNullOrEmpty(name) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexDir}/{name}.png");
        static Color C(float[] a, float alpha = 1f) => a == null ? Color.white : new Color(a[0], a[1], a[2], a.Length > 3 ? a[3] : alpha);

        // ------------------------------------------------------------------ materials
        public static Dictionary<string, Material> BuildMaterials()
        {
            EnsureFolder(MatDir);
            var pipe = CurrentPipeline();
            var result = new Dictionary<string, Material>();
            Shader sh = pipe == Pipeline.URP ? Shader.Find("Universal Render Pipeline/Lit")
                      : pipe == Pipeline.HDRP ? Shader.Find("HDRP/Lit") : Shader.Find("Standard");
            if (sh == null) { Debug.LogError("[BackroomsQueue] Lit shader not found for " + pipe); return result; }
            foreach (var d in Defs().materials)
            {
                var path = $"{MatDir}/{d.name}.mat";
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); }
                else m.shader = sh;
                m.shaderKeywords = Array.Empty<string>();
                Setup(m, d, pipe);
                EditorUtility.SetDirty(m);
                result[d.name] = m;
            }
            AssetDatabase.SaveAssets();
            return result;
        }

        static void Setup(Material m, MatDef d, Pipeline p)
        {
            bool water = d.tex == "T_Water";
            bool hasTex = !string.IsNullOrEmpty(d.tex) && !water;
            var baseTex = hasTex ? Tex(d.tex + "_BaseColor") : null;
            var nrm = water ? Tex("T_Water_Normal") : (hasTex ? Tex(d.tex + "_Normal") : null);
            var ms = hasTex ? Tex(d.tex + "_MetallicSmoothness") : null;
            var mask = hasTex ? Tex(d.tex + "_MaskMap") : null;
            var em = hasTex ? Tex(d.tex + "_Emission") : null;
            var col = C(d.color);
            var tiling = d.tiling != null && d.tiling.Length == 2 ? new Vector2(d.tiling[0], d.tiling[1]) : Vector2.one;
            var emc = em ? Color.white : C(d.emissionColor);
            if (p == Pipeline.URP)
            {
                m.SetTexture("_BaseMap", baseTex); m.SetColor("_BaseColor", col); m.SetTextureScale("_BaseMap", tiling);
                if (nrm) { m.SetTexture("_BumpMap", nrm); m.EnableKeyword("_NORMALMAP"); }
                if (ms) { m.SetTexture("_MetallicGlossMap", ms); m.EnableKeyword("_METALLICSPECGLOSSMAP"); m.SetFloat("_Smoothness", 1f); }
                else { m.SetFloat("_Metallic", d.metallic); m.SetFloat("_Smoothness", d.smoothness); }
                if (mask) { m.SetTexture("_OcclusionMap", mask); m.EnableKeyword("_OCCLUSIONMAP"); m.SetFloat("_OcclusionStrength", 1f); }
                if (d.emission > 0)
                {
                    if (em) m.SetTexture("_EmissionMap", em);
                    m.SetColor("_EmissionColor", emc * d.emission); m.EnableKeyword("_EMISSION");
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                }
                if (d.clip) { m.SetFloat("_AlphaClip", 1); m.SetFloat("_Cutoff", 0.5f); m.EnableKeyword("_ALPHATEST_ON"); m.SetOverrideTag("RenderType", "TransparentCutout"); m.renderQueue = (int)RenderQueue.AlphaTest; m.SetFloat("_Cull", 0); }
                if (d.blend)
                {
                    m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0); m.SetOverrideTag("RenderType", "Transparent");
                    m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    m.SetFloat("_ZWrite", 0); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue = (int)RenderQueue.Transparent;
                }
            }
            else if (p == Pipeline.BuiltIn)
            {
                m.SetTexture("_MainTex", baseTex); m.SetColor("_Color", col); m.SetTextureScale("_MainTex", tiling);
                if (nrm) { m.SetTexture("_BumpMap", nrm); m.EnableKeyword("_NORMALMAP"); }
                if (ms) { m.SetTexture("_MetallicGlossMap", ms); m.EnableKeyword("_METALLICGLOSSMAP"); m.SetFloat("_GlossMapScale", 1f); }
                else { m.SetFloat("_Metallic", d.metallic); m.SetFloat("_Glossiness", d.smoothness); }
                if (mask) m.SetTexture("_OcclusionMap", mask);
                if (d.emission > 0)
                {
                    if (em) m.SetTexture("_EmissionMap", em);
                    m.SetColor("_EmissionColor", emc * d.emission); m.EnableKeyword("_EMISSION");
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                }
                if (d.clip)
                {
                    m.SetFloat("_Mode", 1); m.SetOverrideTag("RenderType", "TransparentCutout");
                    m.SetFloat("_SrcBlend", (float)BlendMode.One); m.SetFloat("_DstBlend", (float)BlendMode.Zero); m.SetFloat("_ZWrite", 1f);
                    m.EnableKeyword("_ALPHATEST_ON"); m.renderQueue = (int)RenderQueue.AlphaTest;
                }
                if (d.blend)
                {
                    bool glassy = d.smoothness > 0.85f;
                    m.SetFloat("_Mode", glassy ? 3 : 2); m.SetOverrideTag("RenderType", "Transparent");
                    m.SetFloat("_SrcBlend", (float)(glassy ? BlendMode.One : BlendMode.SrcAlpha));
                    m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite", 0f);
                    m.EnableKeyword(glassy ? "_ALPHAPREMULTIPLY_ON" : "_ALPHABLEND_ON"); m.renderQueue = (int)RenderQueue.Transparent;
                }
            }
            else
            {
                m.SetTexture("_BaseColorMap", baseTex); m.SetColor("_BaseColor", col); m.SetTextureScale("_BaseColorMap", tiling);
                if (nrm) { m.SetTexture("_NormalMap", nrm); m.EnableKeyword("_NORMALMAP"); m.EnableKeyword("_NORMALMAP_TANGENT_SPACE"); }
                if (mask) { m.SetTexture("_MaskMap", mask); m.EnableKeyword("_MASKMAP"); m.SetFloat("_MetallicRemapMax", 1f); m.SetFloat("_SmoothnessRemapMax", 1f); m.SetFloat("_AORemapMax", 1f); }
                else { m.SetFloat("_Metallic", d.metallic); m.SetFloat("_Smoothness", d.smoothness); }
                if (d.emission > 0)
                {
                    if (em) { m.SetTexture("_EmissiveColorMap", em); m.EnableKeyword("_EMISSIVE_COLOR_MAP"); }
                    m.SetColor("_EmissiveColor", emc * d.emission * 1000f); m.SetColor("_EmissiveColorLDR", emc);
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                }
                if (d.clip) { m.SetFloat("_AlphaCutoffEnable", 1); m.SetFloat("_AlphaCutoff", 0.5f); m.EnableKeyword("_ALPHATEST_ON"); m.SetFloat("_DoubleSidedEnable", 1); }
                if (d.blend) { m.SetFloat("_SurfaceType", 1); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue = (int)RenderQueue.Transparent; }
                var t = Type.GetType("UnityEditor.Rendering.HighDefinition.HDShaderUtils, Unity.RenderPipelines.HighDefinition.Editor");
                var mi = t?.GetMethod("ResetMaterialKeywords", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(Material) }, null);
                mi?.Invoke(null, new object[] { m });
            }
        }

        // ------------------------------------------------------------------ prefabs
        static GameObject Instantiate(string modelPath, Scene tmp, Dictionary<string, Material> mats, string name)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(model, tmp);
            if (go == null) return null;
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            go.name = name;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var arr = r.sharedMaterials;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] == null) continue;
                    var key = arr[i].name.Replace(" (Instance)", "");
                    if (mats.TryGetValue(key, out var mm)) arr[i] = mm;
                }
                r.sharedMaterials = arr;
                if (IsFlat(name)) r.shadowCastingMode = ShadowCastingMode.Off;
            }
            return go;
        }

        public static Dictionary<string, GameObject> BuildPrefabs(Dictionary<string, Material> mats)
        {
            var result = new Dictionary<string, GameObject>();
            if (!AssetDatabase.IsValidFolder(ModelDir)) { Debug.LogError($"[BackroomsQueue] Missing folder {ModelDir}"); return result; }
            var lod1Of = Defs().lods.ToDictionary(l => l.lod0, l => l.lod1);
            var models = AssetDatabase.FindAssets("t:Model", new[] { ModelDir }).Select(AssetDatabase.GUIDToAssetPath)
                         .Where(pth => Path.GetFileNameWithoutExtension(pth).StartsWith("SM_"))
                         .ToDictionary(pth => Path.GetFileNameWithoutExtension(pth), pth => pth);
            var tmp = EditorSceneManager.NewPreviewScene();
            try
            {
                foreach (var kv in models)
                {
                    var name = kv.Key; var path = kv.Value;
                    if (name.EndsWith("_LOD1")) continue;                       // folded into the LOD0 prefab
                    var rel = Path.GetDirectoryName(path).Replace('\\', '/').Substring(ModelDir.Length).TrimStart('/');
                    var outDir = string.IsNullOrEmpty(rel) ? PrefabDir : $"{PrefabDir}/{rel}";
                    EnsureFolder(outDir);
                    { var existing = AssetDatabase.LoadAssetAtPath<GameObject>($"{outDir}/{name}.prefab"); if (existing != null) { result[name] = existing; continue; } }
                    var go = Instantiate(path, tmp, mats, name);
                    if (go == null) continue;
                    if (lod1Of.TryGetValue(name, out var lodName) && models.TryGetValue(lodName, out var lodPath))
                    {
                        var lod = Instantiate(lodPath, tmp, mats, lodName);
                        if (lod != null)
                        {
                            lod.transform.SetParent(go.transform, false);
                            var group = go.AddComponent<LODGroup>();
                            var r0 = go.GetComponentsInChildren<Renderer>().Where(r => !r.transform.IsChildOf(lod.transform)).ToArray();
                            var r1 = lod.GetComponentsInChildren<Renderer>();
                            group.SetLODs(new[] { new LOD(0.18f, r0), new LOD(0.02f, r1) });
                            group.RecalculateBounds();
                        }
                    }
                    var mf = go.GetComponentInChildren<MeshFilter>();
                    if (mf != null && !IsFlat(name))
                    {
                        if (IsStructure(name))
                        {
                            var mc = mf.gameObject.GetComponent<MeshCollider>();
                            if (mc == null) mc = mf.gameObject.AddComponent<MeshCollider>();
                            mc.sharedMesh = mf.sharedMesh;
                            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic |
                                StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic);
                        }
                        else if (mf.gameObject.GetComponent<Collider>() == null)
                        {
                            var bc = mf.gameObject.AddComponent<BoxCollider>();
                            bc.center = mf.sharedMesh.bounds.center; bc.size = mf.sharedMesh.bounds.size;
                        }
                    }
                    var saved = PrefabUtility.SaveAsPrefabAsset(go, $"{outDir}/{name}.prefab");
                    UnityEngine.Object.DestroyImmediate(go);
                    if (saved != null) result[name] = saved;
                }
            }
            finally { EditorSceneManager.ClosePreviewScene(tmp); }
            AssetDatabase.SaveAssets();
            return result;
        }

        // ------------------------------------------------------------------ demo scenes
        [Serializable] public class MatOverride { public string from; public string to; }
        [Serializable] public class Item { public string asset; public string name; public float[] position; public float rotationY; public float[] rotation; public float[] scale; public MatOverride[] materialOverrides; }
        [Serializable] public class LightItem { public string type; public float[] position; public float[] color; public float energyW; public float[] size; public float spotAngle; public float[] forward; public float range; }
        [Serializable] public class CamItem { public string name; public float[] position; public float[] forward; public float fov; }
        [Serializable] public class Layout { public string scene; public Item[] objects; public LightItem[] lights; public CamItem[] cameras; public float[] ambient; }

        public static void BuildScenes(Dictionary<string, GameObject> prefabs, Dictionary<string, Material> mats)
        {
            if (!AssetDatabase.IsValidFolder(LayoutDir)) { Debug.LogError($"[BackroomsQueue] Missing folder {LayoutDir}"); return; }
            EnsureFolder(SceneDir + "/Grouped");
            var pipe = CurrentPipeline();
            foreach (var guid in AssetDatabase.FindAssets("t:TextAsset", new[] { LayoutDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".json") || path.EndsWith("Materials.json")) continue;
                var layout = JsonUtility.FromJson<Layout>(File.ReadAllText(path));
                if (layout == null || layout.objects == null) { Debug.LogError($"[BackroomsQueue] Could not read layout {path}"); continue; }
                if (string.IsNullOrEmpty(layout.scene)) layout.scene = Path.GetFileNameWithoutExtension(path);
                if (!s_ForceScenes && File.Exists($"{SceneDir}/Grouped/{layout.scene}.unity")) continue;
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var root = new GameObject(layout.scene);
                var groups = new Dictionary<string, Transform>();
                Transform Group(string n)
                {
                    if (!groups.TryGetValue(n, out var t)) { t = new GameObject(n).transform; t.SetParent(root.transform, false); groups[n] = t; }
                    return t;
                }
                int missing = 0;
                foreach (var it in layout.objects)
                {
                    if (!prefabs.TryGetValue(it.asset, out var pf)) { missing++; continue; }
                    var g = IsStructure(it.asset) ? "Structure" : it.asset.Contains("_Decal_") ? "Decals" : "Props";
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(pf, Group(g));
                    go.name = it.name;
                    go.transform.position = new Vector3(it.position[0], it.position[1], it.position[2]);
                    go.transform.rotation = (it.rotation != null && it.rotation.Length == 4)
                        ? new Quaternion(it.rotation[0], it.rotation[1], it.rotation[2], it.rotation[3])
                        : Quaternion.Euler(0f, it.rotationY, 0f);
                    go.transform.localScale = new Vector3(it.scale[0], it.scale[1], it.scale[2]);
                    if (it.materialOverrides != null && it.materialOverrides.Length > 0)
                        foreach (var r in go.GetComponentsInChildren<Renderer>())
                        {
                            var arr = r.sharedMaterials;
                            foreach (var mo in it.materialOverrides)
                                for (int i = 0; i < arr.Length; i++)
                                    if (arr[i] != null && arr[i].name == mo.from && mats.TryGetValue(mo.to, out var nm)) arr[i] = nm;
                            r.sharedMaterials = arr;
                        }
                }
                var lights = Group("Lights");
                if (layout.lights != null)
                    foreach (var l in layout.lights)
                    {
                        var lo = new GameObject(l.type == "AREA" ? "Light_Panel" : l.type == "SPOT" ? "Light_Spot" : "Light_Point");
                        lo.transform.SetParent(lights, false);
                        lo.transform.position = new Vector3(l.position[0], l.position[1] - (l.type == "AREA" ? 0.15f : 0f), l.position[2]);
                        var lt = lo.AddComponent<Light>();
                        if (pipe == Pipeline.HDRP)
                        {
                            var hd = Type.GetType("UnityEngine.Rendering.HighDefinition.HDAdditionalLightData, Unity.RenderPipelines.HighDefinition.Runtime");
                            if (hd != null && lo.GetComponent(hd) == null) lo.AddComponent(hd);
                        }
                        var fwd = l.forward != null && l.forward.Length == 3 ? new Vector3(l.forward[0], l.forward[1], l.forward[2]) : Vector3.down;
                        if (l.type == "SPOT")
                        {
                            lt.type = LightType.Spot; lt.spotAngle = Mathf.Clamp(l.spotAngle, 20f, 160f);
                            lo.transform.rotation = Quaternion.LookRotation(fwd.sqrMagnitude > 1e-6f ? fwd : Vector3.down);
                        }
                        else lt.type = LightType.Point;
                        lt.color = new Color(l.color[0], l.color[1], l.color[2]);
                        bool big = l.type == "AREA" && l.size != null && l.size[0] > 2f;
                        lt.range = l.range > 0f ? l.range : (big ? 14f : l.type == "AREA" ? 7f : l.type == "SPOT" ? 9f : 5f);
                        lt.intensity = pipe == Pipeline.HDRP ? l.energyW * 25f / (4f * Mathf.PI) : Mathf.Clamp(l.energyW / 70f, 0.3f, 4f);
                        lt.shadows = LightShadows.None;
                        lt.lightmapBakeType = LightmapBakeType.Mixed;
                    }
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = layout.ambient != null && layout.ambient.Length >= 3 ? new Color(layout.ambient[0], layout.ambient[1], layout.ambient[2]) : new Color(0.1f, 0.1f, 0.1f);
                var cam = new GameObject("Main Camera"); cam.tag = "MainCamera";
                var c = cam.AddComponent<Camera>(); c.nearClipPlane = 0.05f; c.fieldOfView = 70f;
                cam.AddComponent<AudioListener>();
                if (layout.cameras != null && layout.cameras.Length > 0)
                {
                    var c0 = layout.cameras[0];
                    cam.transform.position = new Vector3(c0.position[0], c0.position[1], c0.position[2]);
                    cam.transform.rotation = Quaternion.LookRotation(new Vector3(c0.forward[0], c0.forward[1], c0.forward[2]));
                    c.fieldOfView = Mathf.Clamp(c0.fov, 40f, 90f);
                    var views = Group("Views");
                    foreach (var cv in layout.cameras.Skip(1))
                    {
                        var v = new GameObject(cv.name); v.transform.SetParent(views, false);
                        v.transform.position = new Vector3(cv.position[0], cv.position[1], cv.position[2]);
                        v.transform.rotation = Quaternion.LookRotation(new Vector3(cv.forward[0], cv.forward[1], cv.forward[2]));
                    }
                }
                EditorSceneManager.SaveScene(scene, $"{SceneDir}/Grouped/{layout.scene}.unity");
                if (missing > 0) Debug.LogWarning($"[BackroomsQueue] {layout.scene}: {missing} objects skipped (prefab not found)");
            }
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
