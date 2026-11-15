using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject audioPanel;
    [SerializeField] string menuScene = "MainMenu";
    [SerializeField] GameObject background;

    bool paused = false;

    void Start()
    {
        ShowPanel(null);
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
            else if (audioPanel.activeSelf)
            {
                BackToSettings();
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

        ShowPanel(mainPanel);
        background.SetActive(true);
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1;

        ShowPanel(null);
        background.SetActive(false);
    }

    public void OpenSettings()
    {
        ShowPanel(settingsPanel);
    }

    public void Back()
    {
        ShowPanel(mainPanel);
    }

    public void OpenAudio()
    {
        ShowPanel(audioPanel);
    }

    public void BackToSettings()
    {
        SoundSettings.Save();
        ShowPanel(settingsPanel);
    }

    public void BackToMenu()
    {
        Resume();
        SceneManager.LoadScene(menuScene);
    }

    void ShowPanel(GameObject panel)
    {
        mainPanel.SetActive(panel == mainPanel);
        settingsPanel.SetActive(panel == settingsPanel);
        audioPanel.SetActive(panel == audioPanel);
    }
}