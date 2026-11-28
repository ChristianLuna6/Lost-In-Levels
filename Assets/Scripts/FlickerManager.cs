using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FlickerManager : MonoBehaviour
{
    [Header("Luces del nivel")]
    public Transform lightsParent;
    public float calmChancePerSecond = 0.02f;
    public float panicChancePerSecond = 1.5f;
    [Range(0.05f, 1f)]
    public float darkestDim = 0.25f;

    [Header("Luz direccional")]
    public Light directionalLight;
    [Range(0.05f, 1f)]
    public float directionalMinFactor = 0.3f;

    [Header("Viñeta")]
    public Volume volume;
    public float vignetteCalm = 0.15f;
    public float vignettePanic = 0.6f;

    class Flicker
    {
        public Light light;
        public float baseIntensity;
        public float timer;
    }

    readonly List<Flicker> flickers = new List<Flicker>();
    float directionalBase;
    Vignette vignette;

    void Start()
    {
        Light[] lights = lightsParent != null
            ? lightsParent.GetComponentsInChildren<Light>()
            : FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (var l in lights)
        {
            if (l.type == LightType.Directional) continue;
            flickers.Add(new Flicker { light = l, baseIntensity = l.intensity });
        }

        if (directionalLight != null) directionalBase = directionalLight.intensity;

        if (volume != null && volume.profile != null && volume.profile.TryGet(out vignette))
        {
            vignette.active = true;
            vignette.intensity.overrideState = true;
        }
    }

    void Update()
    {
        float sanity = SanityManager.Instance != null ? SanityManager.Instance.Sanity : 1f;
        float stress = 1f - sanity;
        float chance = Mathf.Lerp(calmChancePerSecond, panicChancePerSecond, stress);
        float dim = Mathf.Lerp(1f, darkestDim, stress);

        foreach (var f in flickers)
        {
            if (f.light == null) continue;

            if (f.timer > 0f)
            {
                f.timer -= Time.deltaTime;
                f.light.intensity = f.baseIntensity * Random.Range(0f, 0.35f);
                continue;
            }

            f.light.intensity = f.baseIntensity * dim;

            if (Random.value < chance * Time.deltaTime)
                f.timer = Random.Range(0.05f, 0.25f);
        }

        if (directionalLight != null)
            directionalLight.intensity = directionalBase * Mathf.Lerp(1f, directionalMinFactor, stress);

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(vignetteCalm, vignettePanic, stress);
    }
}
