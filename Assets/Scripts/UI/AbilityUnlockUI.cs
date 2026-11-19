using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUnlockUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject unlockPanel;
    [SerializeField] private TMP_Text abilityDescriptionText;
    [SerializeField] private Button continueButton;

    [Header("Optional Player Freeze")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;

    private bool isSubscribed = false;

    private void Awake()
    {
        if (unlockPanel != null)
            unlockPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(ClosePopup);
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (isSubscribed && PlayerAbilityManager.Instance != null)
        {
            PlayerAbilityManager.Instance.OnAbilitiesUnlocked -= ShowUnlockPopup;
            isSubscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (isSubscribed || PlayerAbilityManager.Instance == null) return;

        PlayerAbilityManager.Instance.OnAbilitiesUnlocked += ShowUnlockPopup;
        isSubscribed = true;
    }

    private void ShowUnlockPopup(AbilityType unlockedAbilities)
    {
        if (unlockedAbilities == AbilityType.None) return;

        BuildDisplayText(unlockedAbilities, out string description);


        if (abilityDescriptionText != null)
            abilityDescriptionText.text = description;

        SetPlayerControlEnabled(false);

        if (unlockPanel != null)
            unlockPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        if (unlockPanel != null)
            unlockPanel.SetActive(false);

        SetPlayerControlEnabled(true);
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (playerInput != null) playerInput.enabled = enabled;
        if (playerMovement != null) playerMovement.enabled = enabled;
    }

    private void BuildDisplayText(AbilityType abilities, out string description)
    {
        List<string> names = new List<string>();
        List<string> descriptions = new List<string>();

        if ((abilities & AbilityType.DoubleJump) == AbilityType.DoubleJump)
        {
            names.Add("Double Jump");
            descriptions.Add("• <b>Double Jump:</b> Press Jump again while mid-air to reach higher ledges.");
        }

        if ((abilities & AbilityType.Dash) == AbilityType.Dash)
        {
            names.Add("Dash");
            descriptions.Add("• <b>Dash:</b> Press Dash to burst horizontally across gaps or away from walls.");
        }

        if ((abilities & AbilityType.GrapplingHook) == AbilityType.GrapplingHook)
        {
            names.Add("Grappling Hook");
            descriptions.Add("• <b>Grappling Hook:</b> Latch onto anchor points to swing across chasms.");
        }

        if ((abilities & AbilityType.Glide) == AbilityType.Glide)
        {
            names.Add("Glide");
            descriptions.Add("• <b>Glide:</b> Hold Jump in mid-air to slow your descent and ride updrafts.");
        }

        description = string.Join("\n", descriptions);
    }
}