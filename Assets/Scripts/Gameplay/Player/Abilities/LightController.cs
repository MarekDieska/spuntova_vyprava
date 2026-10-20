using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class LightController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float depleteRate = 20f;
    [SerializeField] private float rechargeRate = 15f;

    [SerializeField] private float minimumEnergyToRestart = 20f;

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
                Debug.Log("Light Turned ON");
            }

            currentEnergy -= depleteRate * Time.deltaTime;

            if (currentEnergy <= 0)
            {
                currentEnergy = 0;
                isOverheated = true;
                isLightActive = false;
                Debug.Log("Light DIED - Overheated! Waiting for recharge...");
            }
        }
        else
        {
            if (isLightActive)
            {
                isLightActive = false;
                Debug.Log("Light Turned OFF");
            }

            if (currentEnergy < maxEnergy)
            {
                currentEnergy += rechargeRate * Time.deltaTime;

                if (isOverheated && currentEnergy >= minimumEnergyToRestart)
                {
                    isOverheated = false;
                    Debug.Log("Light recovered enough energy to use again.");
                }
            }
        }

        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
    }

    private void AimLight()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(input.MouseScreenPosition);
        mouseWorldPos.z = 0f;

        Vector3 aimDirection = (mouseWorldPos - transform.position).normalized;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;


        Debug.DrawRay(transform.position, aimDirection * 5f, Color.yellow); // Visual debug line
    }
}