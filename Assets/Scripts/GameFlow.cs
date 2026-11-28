using UnityEngine.SceneManagement;

// Orden de escenas en File > Build Profiles:
//   0 = MainMenu, 1 = Level1, 2 = Level2, 3 = Final
public static class GameFlow
{
    public const int FirstLevelIndex = 1;

    // 0 = Level1, 1 = Level2
    public static int LevelNumber
    {
        get { return SceneManager.GetActiveScene().buildIndex - FirstLevelIndex; }
    }

    public static void LoadFirstLevel()
    {
        SceneManager.LoadScene(FirstLevelIndex);
    }

    public static void LoadMenu()
    {
        SceneManager.LoadScene(0);
    }
}