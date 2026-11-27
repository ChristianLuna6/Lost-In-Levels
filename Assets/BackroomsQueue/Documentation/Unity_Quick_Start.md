# Backrooms - The Queue - Unity quick start

Made for Unity 6 and the Universal Render Pipeline (URP).

1. Import `Backrooms_Queue_Unity.unitypackage` (Assets > Import Package > Custom Package).
2. Open a scene from `Assets/BackroomsQueue/Scenes/`:
   - `Grouped/V3_Queue_ThousandWaits` - the finished level, every piece assembled
   - `Separated/BackroomsQueue_All_Assets` - every prefab on its own, in rows by category (the Views object has a camera spot per category)
3. Prefabs are in `Assets/BackroomsQueue/Prefabs/`, sorted like the models. Structure pieces carry mesh colliders and static flags;
   props carry box colliders; decals and paper have no collider and cast no shadows. Props that have a lower-detail
   mesh come with a LOD Group (LOD0 + LOD1).
4. Materials are URP/Lit, in `Assets/BackroomsQueue/Materials/`. Lightmap UVs are already in the meshes (UV1), so leave
   *Generate Lightmap UVs* off and bake with Window > Rendering > Lighting.

## Built-in Render Pipeline or HDRP
Run **Tools > Backrooms Queue > Build Everything** once. It converts the materials to the active pipeline (the prefabs and both kinds of scene pick
them up automatically) and recreates any prefab or grouped scene that is missing. On HDRP, also run
**Tools > Backrooms Queue > Rebuild Grouped Scenes (overwrite)** so the level lights use HDRP light units.

`README.md` has the full asset list, sizes, grid and pivot conventions and texture map details.
