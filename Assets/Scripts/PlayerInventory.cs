using UnityEngine;
using UnityEngine.SceneManagement;

// Guarda si el jugador tiene la llave. Se vacía solo al cargar cualquier escena.
public static class PlayerInventory
{
    public static bool HasKey;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) => HasKey = false;
    }
}
