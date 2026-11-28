using UnityEngine;
using UnityEngine.SceneManagement;


public static class DifficultyManager
{
    static readonly float[] drainSeconds = { 70f, 55f, 120f };

    static readonly float[] waterRestore = { 0.4f, 0.4f, 0.4f };

    static readonly int[] waterCount = { 2, 2, 1 };

    public static int Level
    {
        get { return Mathf.Clamp(SceneManager.GetActiveScene().buildIndex, 0, 2); }
    }

    public static float DrainSeconds { get { return drainSeconds[Level]; } }
    public static float WaterRestore { get { return waterRestore[Level]; } }
    public static int WaterCount { get { return waterCount[Level]; } }

    public static float Current { get { return Level / 2f; } }
}
