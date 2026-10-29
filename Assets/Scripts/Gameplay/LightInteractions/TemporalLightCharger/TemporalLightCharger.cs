using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class TemporalLightCharger : MonoBehaviour, ILightReactable
{
    [Header("Charge Settings")]
    [SerializeField] private float timeToCharge = 2f;
    [SerializeField] private float activeDuration = 5f;
    [SerializeField] private float dropRate = 1f;
    [Header("Level Designer Events")]
    [SerializeField] private UnityEvent<bool> OnChargeStateChangedLevelEvent;

    public event Action<bool> OnChargeStateChangedAction;
    public event Action<float> OnChargeProgressAction;

    private float currentCharge = 0f;
    private float activeTimer = 0f;
    private bool isFullyCharged = false;
    private bool isLit = false;
    public void OnLightEnter() => isLit = true;

    public void OnLightExit() => isLit = false;

    public void OnLightStay(float deltaTime)
    {
        if (isFullyCharged)
        {
            if (activeTimer < activeDuration)
            {
                float rechargeRate = activeDuration / timeToCharge;
                activeTimer = Mathf.Min(activeTimer + (deltaTime * rechargeRate), activeDuration);

                float progress = activeTimer / activeDuration;
                OnChargeProgressAction?.Invoke(progress);
                Debug.Log($"Energy State: {progress * 100f:F1}%");
            }
            else
            {
                activeTimer = activeDuration;
                Debug.Log("Energy State: 100%");
            }
            return;
        }
        currentCharge += deltaTime;
        OnChargeProgressAction?.Invoke(currentCharge / timeToCharge);
        Debug.Log($"Energy State: {(currentCharge / timeToCharge) * 100f:F1}%");
        if (currentCharge >= timeToCharge)
        {
            isFullyCharged = true;
            activeTimer = activeDuration;

            OnChargeStateChangedAction?.Invoke(true); //t or f can be changed dependent on needs
            OnChargeStateChangedLevelEvent?.Invoke(false);
        }
    }
    private void Update()
    {
        if (isFullyCharged && !isLit)
        {
            activeTimer -= Time.deltaTime;
            float progress = Mathf.Clamp01(activeTimer / activeDuration);
            OnChargeProgressAction?.Invoke(progress);
            Debug.Log($"Energy State: {progress * 100f:F1}%");
            if (activeTimer <= 0f)
            {
                isFullyCharged = false;
                currentCharge = 0f;

                OnChargeProgressAction?.Invoke(0f);
                OnChargeStateChangedAction?.Invoke(false); //t or f can be changed dependent on needs
                OnChargeStateChangedLevelEvent?.Invoke(true);
            }
        }
        else if (!isLit && currentCharge > 0f)
        {
            currentCharge = Mathf.Max(0f, currentCharge - (dropRate * Time.deltaTime));
            OnChargeProgressAction?.Invoke(currentCharge / timeToCharge);
            Debug.Log($"Energy State: {(currentCharge / timeToCharge) * 100f:F1}%");
        }
    }
}
