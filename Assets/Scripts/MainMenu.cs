using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI bestTimeText;   // opcional: muestra tu mejor tiempo

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (bestTimeText != null)
        {
            float best = PlayerPrefs.GetFloat("Record0", -1f);
            bestTimeText.text = best > 0f
                ? "Mejor tiempo: " + GameTimer.Format(best)
                : "Aún no has escapado";
        }
    }

    public void Play()
    {
        GameTimer.Instance.ResetTimer();
        GameFlow.LoadFirstLevel();
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}