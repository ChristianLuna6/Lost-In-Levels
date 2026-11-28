using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public float spinSpeed = 60f;   // pon 0 si no quieres que gire

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInventory.HasKey = true;

        if (LevelManager.Instance != null)
            LevelManager.Instance.ShowMessage("Encontraste una llave", 2.5f);

        Destroy(gameObject);
    }
}