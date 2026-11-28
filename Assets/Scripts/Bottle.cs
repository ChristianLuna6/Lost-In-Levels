using UnityEngine;

public class Bottle : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CollectBottle();
        }
        else
        {
            Debug.LogWarning("Falta el LevelManager en la escena.");
        }

        Destroy(gameObject);
    }
}