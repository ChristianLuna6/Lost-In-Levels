using UnityEngine;

public class AlmondWater : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (SanityManager.Instance != null)
            SanityManager.Instance.Restore(DifficultyManager.WaterRestore);

        if (LevelManager.Instance != null)
            LevelManager.Instance.ShowMessage("Agua de almendras: recuperas cordura", 2f);

        Destroy(gameObject);
    }
}