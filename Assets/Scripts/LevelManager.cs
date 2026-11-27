using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Latas")]
    public int bottlesNeeded = 7; // pon 0 para contar automáticamente las latas de la escena
    public TextMeshProUGUI bottleCounterText;

    [Header("Mensajes")]
    public TextMeshProUGUI messageText;
    public CanvasGroup messageGroup;
    public string introMessage = "Mantén el sitio limpio";

    [Header("Salida")]
    public GameObject unlockIcon;
    public ExitDoor[] exitDoors;

    int collected;
    bool completed;
    Coroutine messageRoutine;

    void Awake() { Instance = this; }

    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            GameTimer.Instance.ResetTimer();

        if (bottlesNeeded <= 0)
            bottlesNeeded = FindObjectsByType<Bottle>(FindObjectsSortMode.None).Length;

        if (unlockIcon != null) unlockIcon.SetActive(false);
        UpdateCounter();
        ShowMessage(introMessage + "\nRecoge " + bottlesNeeded + " latas", 5f);
    }

    public void CollectBottle()
    {
        if (completed) return;
        collected++;
        UpdateCounter();
        if (collected >= bottlesNeeded) CompleteTask();
    }

    void CompleteTask()
    {
        completed = true;
        if (unlockIcon != null) unlockIcon.SetActive(true);
        if (exitDoors != null && exitDoors.Length > 0)
            exitDoors[Random.Range(0, exitDoors.Length)].Unlock(); // abre una de las dos al azar
        ShowMessage("Todo limpio. Una salida se ha abierto", 4f);
    }

    void UpdateCounter()
    {
        if (bottleCounterText != null)
            bottleCounterText.text = collected + "/" + bottlesNeeded;
    }

    public void ShowMessage(string text, float seconds)
    {
        if (messageText == null || messageGroup == null) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(text, seconds));
    }

    IEnumerator MessageRoutine(string text, float seconds)
    {
        messageText.text = text;
        messageGroup.alpha = 1f;
        yield return new WaitForSeconds(seconds);
        while (messageGroup.alpha > 0f)
        {
            messageGroup.alpha -= Time.deltaTime;
            yield return null;
        }
    }
}
