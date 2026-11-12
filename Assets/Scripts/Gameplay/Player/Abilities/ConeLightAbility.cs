using UnityEngine;
using UnityEngine.InputSystem;

public class ConeLightAbility : BaseLightAbility
{
    [Header("Cone Settings")]
    [SerializeField] private float rotationOffset = -90f;
    [SerializeField] private float stickDeadzone = 0.25f;

    private bool usingStick;

    protected override void UpdateLightBehavior(Vector3 mouseWorldPos)
    {
        if (abilityLight == null) return;

        Vector2 stick = Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() : Vector2.zero;

        if (stick.magnitude > stickDeadzone)
        {
            usingStick = true;
            AimDirection = stick;
        }
        else if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            usingStick = false;
        }

        if (!usingStick)
            AimDirection = mouseWorldPos - transform.position;

        float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
        abilityLight.transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
    }
}
