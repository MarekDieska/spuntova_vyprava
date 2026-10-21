
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light2D circleLight;
    [SerializeField] private float lightRadius = 5f;
    [SerializeField] private Key toggleKey = Key.F;

    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float depleteRate = 20f;
    [SerializeField] private float rechargeRate = 15f;
    [SerializeField] private float minimumEnergyToRestart = 20f;

    private Camera mainCamera;
    private float currentEnergy;
    private bool isLightActive = true;
    private bool isOverheated;

    private void Awake()
    {
        mainCamera = Camera.main;
        currentEnergy = maxEnergy;

        if (circleLight != null)
        {
            circleLight.enabled = false;
            circleLight.pointLightOuterRadius = lightRadius;
        }
    }

    private void Update()
    {
        HandleInput();
        HandleEnergy();

        if (circleLight != null)
        {
            circleLight.enabled = isLightActive;

            if (isLightActive)
            {
                MoveLightToMouse();
            }
        }
    }

    private void HandleInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[toggleKey].wasPressedThisFrame && !isOverheated)
        {
            isLightActive = !isLightActive;
        }
    }

    private void HandleEnergy()
    {
        if (isLightActive)
        {
            currentEnergy -= depleteRate * Time.deltaTime;

            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                isLightActive = false;
                isOverheated = true;
            }
        }
        else
        {
            currentEnergy = Mathf.Min(
                currentEnergy + rechargeRate * Time.deltaTime,
                maxEnergy
            );

            if (isOverheated &&
                currentEnergy >= minimumEnergyToRestart)
            {
                isOverheated = false;
            }
        }
    }

    private void MoveLightToMouse()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        float cameraDistance = Mathf.Abs(
            mainCamera.transform.position.z
        );

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                mouseScreenPosition.x,
                mouseScreenPosition.y,
                cameraDistance
            )
        );

        mouseWorldPosition.z = 0f;

        circleLight.transform.position = mouseWorldPosition;
    }

    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public float GetEnergyPercentage()
    {
        return maxEnergy > 0f ? currentEnergy / maxEnergy : 0f;
    }
}
