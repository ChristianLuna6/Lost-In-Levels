using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;      // panel con los botones Continuar y Salir al menú
    public GameObject touchControls;   // arrastra aquí UI_TouchScreenInput

    bool paused;

    void Start()
    {
        SetPaused(false);
    }

    void Update()
    {
        // En PC, Esc también pausa
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetPaused(!paused);
    }

    public void Pause() { SetPaused(true); }
    public void Resume() { SetPaused(false); }

    public void ToMenu()
    {
        Time.timeScale = 1f;
        GameFlow.LoadMenu();
    }

    void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = value ? 0f : 1f;   // congela el juego, el cronómetro y la cordura
        if (pausePanel != null) pausePanel.SetActive(value);
        if (touchControls != null) touchControls.SetActive(!value);   // oculta los joysticks en pausa

        Cursor.visible = value;
        Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
    }
}