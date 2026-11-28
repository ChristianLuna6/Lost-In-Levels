using UnityEngine;

// Valores por nivel: { Level1, Level2, (sin usar) }
public static class DifficultyManager
{
    // Segundos que tarda la barra de cordura en vaciarse
    static readonly float[] drainSeconds = { 60f, 60f, 60f };

    // Cuánta cordura devuelve el agua de almendras (0.4 = 40 % de la barra)
    static readonly float[] waterRestore = { 0.4f, 0.4f, 0.4f };

    // Cuántas botellas de agua de almendras hay activas en cada nivel
    static readonly int[] waterCount = { 2, 2, 1 };

    public static int Level
    {
        get { return Mathf.Clamp(GameFlow.LevelNumber, 0, 2); }
    }

    public static float DrainSeconds { get { return drainSeconds[Level]; } }
    public static float WaterRestore { get { return waterRestore[Level]; } }
    public static int WaterCount { get { return waterCount[Level]; } }

    public static float Current { get { return Level / 2f; } }
}