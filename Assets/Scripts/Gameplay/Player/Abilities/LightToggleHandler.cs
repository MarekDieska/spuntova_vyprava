using UnityEngine;

public class LightToggleHandler : MonoBehaviour
{
    [SerializeField] private bool isBossFight = false;
    private PlayerInput input;
    private ConeLightAbility coneLightAbility;
    private MovableLightAbility movableLightAbility;
    private bool isMovableLightActive = false;
    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        coneLightAbility = GetComponentInChildren<ConeLightAbility>();
        movableLightAbility = GetComponentInChildren<MovableLightAbility>();
    }
    private void Update()
    {
        if (!isBossFight)
        {
            if (isMovableLightActive)
            {
                isMovableLightActive = false;
                ApplyLightState();
            }
            return;
        }
        if (input != null && input.LightTogglePressedThisFrame)
        {
            isMovableLightActive = !isMovableLightActive;
            ApplyLightState();
        }
    }
    private void ApplyLightState()
    {
        if (!isBossFight)
        {
            if (coneLightAbility != null)
                coneLightAbility.enabled = true;
            if (movableLightAbility != null)
                movableLightAbility.enabled = false;
        }
        else
        {
            if (coneLightAbility != null)
                coneLightAbility.enabled = !isMovableLightActive;
            if (movableLightAbility != null)
                movableLightAbility.enabled = isMovableLightActive;
        }
    }
}
