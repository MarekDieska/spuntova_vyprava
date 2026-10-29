using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(TemporalLightCharger))]
public class TemporalLightChargerVisuals : MonoBehaviour
{
    [SerializeField] private Color emptyColor = Color.gray;
    [SerializeField] private Color chargedColor = Color.yellow;

    private SpriteRenderer sr;
    private TemporalLightCharger charger;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        charger = GetComponent<TemporalLightCharger>();
        sr.color = emptyColor;
    }

    private void OnEnable()
    {
        charger.OnChargeProgressAction += UpdateVisuals;
    }

    private void OnDisable()
    {
        charger.OnChargeProgressAction -= UpdateVisuals;
    }

    private void UpdateVisuals(float progressPercentage)
    {
        sr.color = Color.Lerp(emptyColor, chargedColor, progressPercentage);
    }
}
