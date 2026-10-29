using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerInput))]
public abstract class BaseLightAbility : MonoBehaviour
{
    [Header("Light Component")]
    [SerializeField] protected Light2D abilityLight;
    [SerializeField] protected Collider2D lightCollider;
    [Header("Energy Settings")]
    [SerializeField] protected float maxEnergy = 100f;
    [SerializeField] protected float depleteRate = 20f;
    [SerializeField] protected float rechargeRate = 15f;
    [SerializeField] protected float minimumEnergyToRestart = 20f;
    [SerializeField] private bool useUnlimitedEnergy = false;

    protected PlayerInput input;
    protected Camera mainCamera;
    protected float currentEnergy;
    protected bool isLightActive;
    protected bool isOverheated;

    protected virtual void Awake()
    {
        input = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
        currentEnergy = maxEnergy;

        if (abilityLight != null)
            abilityLight.enabled = false;
        if (lightCollider != null)
            lightCollider.enabled = false;
    }

    protected virtual void Update()
    {
        HandleInputAndEnergy();

        if (isLightActive)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            UpdateLightBehavior(mousePos);
        }
    }

    private void HandleInputAndEnergy()
    {
        if (useUnlimitedEnergy)
        {
            currentEnergy = maxEnergy;
            isOverheated = false;
        }
        bool wantsToUseLight = input.LightHeld && !isOverheated;

        if (wantsToUseLight && currentEnergy > 0f)
        {
            if (!isLightActive)
            {
                isLightActive = true;
                ToggleLightVisuals(true);
            }

            if (!useUnlimitedEnergy)
            {
                currentEnergy -= depleteRate * Time.deltaTime;

                if (currentEnergy <= 0f)
                {
                    currentEnergy = 0f;
                    isOverheated = true;
                    TurnOffLight();
                }
            }
        }
        else
        {
            if (isLightActive) TurnOffLight();

            if (!useUnlimitedEnergy && currentEnergy < maxEnergy)
            {
                currentEnergy = Mathf.Min(currentEnergy + rechargeRate * Time.deltaTime, maxEnergy);

                if (isOverheated && currentEnergy >= minimumEnergyToRestart)
                {
                    isOverheated = false;
                }
            }
        }
    }

    private void TurnOffLight()
    {
        isLightActive = false;
        ToggleLightVisuals(false);
    }
    protected virtual void ToggleLightVisuals(bool isActive)
    {
        if (abilityLight != null)
            abilityLight.enabled = isActive;
        if (lightCollider != null)
            lightCollider.enabled = isActive;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) return Vector3.zero;

        Vector2 mouseScreenPosition = input.MouseScreenPosition;
        float cameraDistance = Mathf.Abs(mainCamera.transform.position.z);

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, cameraDistance)
        );
        mouseWorldPosition.z = 0f;

        return mouseWorldPosition;
    }

    protected abstract void UpdateLightBehavior(Vector3 mouseWorldPos);

    public float GetCurrentEnergy() => currentEnergy;
    public float GetEnergyPercentage() => maxEnergy > 0f ? currentEnergy / maxEnergy : 0f;
}