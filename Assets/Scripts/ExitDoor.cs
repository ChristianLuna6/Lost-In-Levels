using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    [Header("Brillo")]
    public Light doorLight;               // luz que se enciende (apagada al inicio)
    public GameObject glowObject;         // opcional: lámina verde (Quad) para puertas pintadas
    public Renderer[] doorRenderers;      // opcional: partes de una puerta 3D que brillan un poco
    public Color glowColor = new Color(0.2f, 1f, 0.3f);
    [Range(0f, 3f)]
    public float emissionStrength = 0.6f; // qué tanto brilla la puerta 3D (0.6 = poquito)

    [Header("Llave (solo la puerta final del nivel 2)")]
    public bool requiresKey = false;      // si está marcado, solo se abre cuando el jugador tiene la llave
    public float loadDelay = 0.5f;        // segundos antes de pasar a la siguiente escena (con llave)

    bool unlocked;
    bool loading;

    void Start()
    {
        if (doorLight != null)
        {
            doorLight.color = glowColor;
            doorLight.enabled = false;
        }
        if (glowObject != null) glowObject.SetActive(false);
    }

    void Update()
    {
        // Con llave: en cuanto la recoges, la puerta empieza a brillar
        if (requiresKey && !unlocked && PlayerInventory.HasKey)
            Unlock();
    }

    public void Unlock()
    {
        if (unlocked) return;
        unlocked = true;

        if (doorLight != null) doorLight.enabled = true;
        if (glowObject != null) glowObject.SetActive(true);

        if (doorRenderers != null)
        {
            foreach (var r in doorRenderers)
            {
                if (r == null) continue;
                foreach (var m in r.materials)
                {
                    m.EnableKeyword("_EMISSION");
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    m.SetColor("_EmissionColor", glowColor * emissionStrength);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (loading || !other.CompareTag("Player")) return;

        if (!unlocked)
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.ShowMessage(requiresKey ? "Necesitas una llave" : "La puerta está cerrada", 2f);
            return;
        }

        loading = true;
        if (requiresKey && loadDelay > 0f) Invoke(nameof(LoadNext), loadDelay);
        else LoadNext();
    }

    void LoadNext()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        GameTimer.Instance.FinishLevel(index);
        SceneManager.LoadScene(index + 1);
    }
}
