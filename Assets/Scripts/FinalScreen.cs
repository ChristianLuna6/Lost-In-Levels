using UnityEngine;
using TMPro;

public class FinalScreen : MonoBehaviour
{
    public TextMeshProUGUI summaryText;
    public TextMeshProUGUI recordsText;
    public int levelsPlayed = 2;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var timer = GameTimer.Instance;
        timer.Stop();

        // Solo guarda el récord si de verdad se jugó
        int position = -1;
        if (timer.Elapsed > 1f)
            position = GameTimer.SaveRecord(timer.Elapsed);

        string summary = "";
        for (int i = 0; i < levelsPlayed && i < timer.levelTimes.Length; i++)
            summary += "Nivel " + (i + 1) + ":   " + GameTimer.Format(timer.levelTimes[i]) + "\n";
        summary += "\nTIEMPO TOTAL:   " + GameTimer.Format(timer.Elapsed);
        if (summaryText != null) summaryText.text = summary;

        var records = GameTimer.LoadRecords();
        string table = "MEJORES TIEMPOS\n\n";
        for (int i = 0; i < records.Count; i++)
        {
            table += (i + 1) + ".   " + GameTimer.Format(records[i]);
            if (i == position) table += "   <  tu tiempo";
            table += "\n";
        }
        if (recordsText != null) recordsText.text = table;
    }

    public void PlayAgain()
    {
        GameTimer.Instance.ResetTimer();
        GameFlow.LoadFirstLevel();
    }

    public void ToMenu()
    {
        GameFlow.LoadMenu();
    }
}