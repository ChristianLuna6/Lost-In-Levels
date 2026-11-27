using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    public Light doorLight;         
    public GameObject glowObject;    
    public Color glowColor = new Color(0.2f, 1f, 0.3f);

    bool unlocked;

    void Start()
    {
        if (doorLight != null)
        {
            doorLight.color = glowColor;
            doorLight.enabled = false;
        }
        if (glowObject != null) glowObject.SetActive(false);
    }

    public void Unlock()
    {
        unlocked = true;
        if (doorLight != null) doorLight.enabled = true;
        if (glowObject != null) glowObject.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!unlocked)
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.ShowMessage("La puerta está cerrada", 2f);
            return;
        }

        int index = SceneManager.GetActiveScene().buildIndex;
        GameTimer.Instance.FinishLevel(index);
        SceneManager.LoadScene(index + 1);
    }
}