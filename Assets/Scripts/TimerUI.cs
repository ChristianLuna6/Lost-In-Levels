using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Update()
    {
        text.text = GameTimer.Format(GameTimer.Instance.Elapsed);
    }
}
