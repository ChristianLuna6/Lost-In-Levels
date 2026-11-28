using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FinalScreen : MonoBehaviour
{
    public TextMeshProUGUI summaryText;   // tiempos de cada nivel y total
    public TextMeshProUGUI recordsText;   // tabla de mejores tiempos
    public int levelsPlayed = 2;          // cuántos niveles tiene tu juego

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var timer = GameTimer.Instance;
        timer.Stop();

        // Solo guarda el récord si de verdad se jugó (evita guardar 0 al probar esta escena sola)
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
        SceneManager.LoadScene(0);
    }
}