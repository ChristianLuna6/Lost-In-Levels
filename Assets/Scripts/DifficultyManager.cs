using UnityEngine;
using UnityEngine.SceneManagement;

// Valores por nivel. El nivel se toma del orden en Build Profiles:
// Level1 = 0, Level2 = 1, Level3 = 2. Cambia los números para ajustar la dificultad.
public static class DifficultyManager
{
    // Segundos que tarda la barra de cordura en vaciarse por completo
    static readonly float[] drainSeconds = { 150f, 135f, 120f };

    // Cuánta cordura devuelve el agua de almendras (0.4 = 40 % de la barra)
    static readonly float[] waterRestore = { 0.4f, 0.4f, 0.4f };

    // Cuántas botellas de agua de almendras hay activas en cada nivel
    static readonly int[] waterCount = { 2, 2, 1 };

    public static int Level
    {
        get { return Mathf.Clamp(SceneManager.GetActiveScene().buildIndex, 0, 2); }
    }

    public static float DrainSeconds { get { return drainSeconds[Level]; } }
    public static float WaterRestore { get { return waterRestore[Level]; } }
    public static int WaterCount { get { return waterCount[Level]; } }

    // 0 en el nivel 1, 0.5 en el nivel 2, 1 en el nivel 3 (útil para luces o efectos)
    public static float Current { get { return Level / 2f; } }
}
