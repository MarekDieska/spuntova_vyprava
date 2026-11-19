using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button quitToMenuButton;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (respawnButton != null)
            respawnButton.onClick.AddListener(OnRespawnClicked);

        if (quitToMenuButton != null)
            quitToMenuButton.onClick.AddListener(OnQuitToMenuClicked);
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDeathAction += ShowDeathUI;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDeathAction -= ShowDeathUI;
    }

    private void ShowDeathUI()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);
    }

    private void OnRespawnClicked()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (playerHealth != null)
            playerHealth.RespawnAfterDeath();
    }

    private void OnQuitToMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}