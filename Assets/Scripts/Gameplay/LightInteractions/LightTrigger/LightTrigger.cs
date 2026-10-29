using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class LightTrigger : MonoBehaviour, ILightReactable
{
    [SerializeField] private UnityEvent<bool> OnLightTriggerEvent;
    public event Action<bool> OnLightTriggerAction;
    private bool isActivated = false;
    public void OnLightEnter()
    {
        if (!isActivated)
        {
            OnLightTriggerEvent?.Invoke(true);
            OnLightTriggerAction?.Invoke(true);
            isActivated = true;
        }
    }

    public void OnLightExit()
    {
        if (isActivated)
        {
            isActivated = false;
            OnLightTriggerEvent?.Invoke(false);
            OnLightTriggerAction?.Invoke(false);
        }
    }

    public void OnLightStay(float deltaTime)
    {
    }
}
