using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerInventory
{
    public static bool HasKey;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) => HasKey = false;
    }
}