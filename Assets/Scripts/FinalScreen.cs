using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FinalScreen : MonoBehaviour
{
    public TextMeshProUGUI summaryText;
    public TextMeshProUGUI recordsText;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var timer = GameTimer.Instance;
        timer.Stop();
        int position = GameTimer.SaveRecord(timer.Elapsed);

        string summary = "Tiempo total: " + GameTimer.Format(timer.Elapsed);
        for (int i = 0; i < timer.levelTimes.Length; i++)
            summary += "\nNivel " + (i + 1) + ": " + GameTimer.Format(timer.levelTimes[i]);
        summaryText.text = summary;

        var records = GameTimer.LoadRecords();
        string table = "MEJORES TIEMPOS";
        for (int i = 0; i < records.Count; i++)
        {
            table += "\n" + (i + 1) + ". " + GameTimer.Format(records[i]);
            if (i == position) table += "  (tu tiempo)";
        }
        recordsText.text = table;
    }

    public void PlayAgain()
    {
        GameTimer.Instance.ResetTimer();
        SceneManager.LoadScene(0);
    }
}
