using UnityEngine;

public class ConeLightAbility : BaseLightAbility
{
    [Header("Cone Settings")]
    [SerializeField] private float rotationOffset = -90f;

    protected override void UpdateLightBehavior(Vector3 mouseWorldPos)
    {
        if (abilityLight == null) return;

        Vector3 aimDirection = (mouseWorldPos - transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        abilityLight.transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
    }
}