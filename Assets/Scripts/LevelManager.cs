using System.Collections;
using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Latas")]
    public int bottlesNeeded = 7; // pon 0 si este nivel no usa latas (por ejemplo, el de la llave)
    public TextMeshProUGUI bottleCounterText;

    [Header("Mensajes")]
    public TextMeshProUGUI messageText;
    public CanvasGroup messageGroup;
    public string introMessage = "Mantén el sitio limpio";
    public string objectiveOverride = ""; // si lo llenas, reemplaza el texto "Recoge N latas"

    [Header("Salida")]
    public GameObject unlockIcon;
    public ExitDoor[] exitDoors;

    int collected;
    bool completed;
    Coroutine messageRoutine;

    void Awake() { Instance = this; }

    void Start()
    {
        if (GameFlow.LevelNumber == 0)
            GameTimer.Instance.ResetTimer();

        bool usesCans = bottlesNeeded > 0 || FindObjectsByType<Bottle>(FindObjectsSortMode.None).Length > 0;
        if (bottlesNeeded <= 0)
            bottlesNeeded = FindObjectsByType<Bottle>(FindObjectsSortMode.None).Length;

        if (unlockIcon != null) unlockIcon.SetActive(false);

        // Si el nivel no usa latas, se oculta el contador
        if (bottleCounterText != null) bottleCounterText.gameObject.SetActive(usesCans);
        UpdateCounter();

        string objective = string.IsNullOrEmpty(objectiveOverride)
            ? "Recoge " + bottlesNeeded + " latas"
            : objectiveOverride;
        ShowMessage(introMessage + "\n" + objective, 5f);
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
            exitDoors[Random.Range(0, exitDoors.Length)].Unlock();
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