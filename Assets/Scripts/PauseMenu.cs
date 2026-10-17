using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] string menuScene = "MainMenu";
    [SerializeField] GameObject background;

    bool paused = false;

    void Start()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(false);
        background.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (paused == false)
            {
                Pause();
            }
            else if (settingsPanel.activeSelf)
            {
                Back();
            }
            else
            {
                Resume();
            }
        }
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0;

        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        background.SetActive(true);
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1;

        mainPanel.SetActive(false);
        background.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void Back()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void BackToMenu()
    {
        Resume();
        SceneManager.LoadScene(menuScene);
    }
}