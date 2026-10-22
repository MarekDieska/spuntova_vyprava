using UnityEngine;

public class MovableLightAbility : BaseLightAbility
{
    [Header("Circle Settings")]
    [SerializeField] private float lightRadius = 5f;

    protected override void Awake()
    {
        base.Awake();

        if (abilityLight != null)
            abilityLight.pointLightOuterRadius = lightRadius;
    }

    protected override void UpdateLightBehavior(Vector3 mouseWorldPos)
    {
        if (abilityLight != null)
        {
            abilityLight.transform.position = mouseWorldPos;
        }
    }
}