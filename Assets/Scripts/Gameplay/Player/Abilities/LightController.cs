using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerInput))]
public class LightController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float depleteRate = 20f;
    [SerializeField] private float rechargeRate = 15f;
    [SerializeField] private float minimumEnergyToRestart = 20f;
    [SerializeField] private Light2D coneLight;
    private PlayerInput input;
    private Camera mainCamera;

    private float currentEnergy;
    private bool isLightActive;
    private bool isOverheated;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
        currentEnergy = maxEnergy;

        if (coneLight != null) coneLight.enabled = false;
    }

    private void Update()
    {
        HandleEnergyLogic();

        if (isLightActive)
        {
            AimLight();
        }
    }

    private void HandleEnergyLogic()
    {
        bool wantsToUseLight = input.LightHeld && !isOverheated;

        if (wantsToUseLight && currentEnergy > 0)
        {
            if (!isLightActive)
            {
                isLightActive = true;
                if (coneLight != null) coneLight.enabled = true;
            }

            currentEnergy -= depleteRate * Time.deltaTime;

            if (currentEnergy <= 0)
            {
                currentEnergy = 0;
                isOverheated = true;
                TurnOffLight();
            }
        }
        else
        {
            if (isLightActive) TurnOffLight();

            if (currentEnergy < maxEnergy)
            {
                currentEnergy += rechargeRate * Time.deltaTime;

                if (isOverheated && currentEnergy >= minimumEnergyToRestart)
                {
                    isOverheated = false;
                }
            }
        }

        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
    }

    private void TurnOffLight()
    {
        isLightActive = false;
        if (coneLight != null) coneLight.enabled = false;
    }

    private void AimLight()
    {
        if (coneLight == null) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(input.MouseScreenPosition);
        mouseWorldPos.z = 0f;

        Vector3 aimDirection = (mouseWorldPos - coneLight.transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        coneLight.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}