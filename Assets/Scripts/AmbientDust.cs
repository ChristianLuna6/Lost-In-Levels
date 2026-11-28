using UnityEngine;

// Polvo flotando alrededor del jugador. Va en un hijo de PlayerCapsule.
[RequireComponent(typeof(ParticleSystem))]
public class AmbientDust : MonoBehaviour
{
    public float areaSize = 14f;       // ancho del área donde flota el polvo
    public int maxParticles = 150;     // cantidad de partículas
    public float size = 0.04f;         // tamaño de cada partícula
    public Color color = new Color(1f, 0.95f, 0.75f, 0.35f);

    void Start()
    {
        var ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size * 1.5f);
        main.startColor = color;
        main.maxParticles = maxParticles;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = maxParticles / 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.position = new Vector3(0f, 1.5f, 0f);
        shape.scale = new Vector3(areaSize, 3f, areaSize);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.15f;
        noise.frequency = 0.3f;

        // Aparece y desaparece suavemente
        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f),
                    new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
        fade.color = g;

        if (!ps.isPlaying) ps.Play();
    }
}