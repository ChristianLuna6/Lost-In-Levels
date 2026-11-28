using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance;

    public Slider sanityBar;         
    public float restartDelay = 3f;   

    float sanity = 1f;
    bool lost;

    public float Sanity { get { return sanity; } }

    void Awake() { Instance = this; }

    void Start() { UpdateBar(); }

    void Update()
    {
        if (lost) return;

        sanity -= Time.deltaTime / DifficultyManager.DrainSeconds;
        if (sanity <= 0f)
        {
            sanity = 0f;
            Lose();
        }
        UpdateBar();
    }

    public void Restore(float amount)
    {
        if (lost) return;
        sanity = Mathf.Clamp01(sanity + amount);
        UpdateBar();
    }

    void UpdateBar()
    {
        if (sanityBar != null) sanityBar.value = sanity;
    }

    void Lose()
    {
        lost = true;
        PlayerPrefs.SetInt("Deaths", PlayerPrefs.GetInt("Deaths", 0) + 1);
        PlayerPrefs.Save();

        if (LevelManager.Instance != null)
            LevelManager.Instance.ShowMessage("Te perdiste en los Backrooms...\nVuelves al inicio", restartDelay);

        StartCoroutine(RestartRoutine());
    }

    IEnumerator RestartRoutine()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(0);
    }
}
