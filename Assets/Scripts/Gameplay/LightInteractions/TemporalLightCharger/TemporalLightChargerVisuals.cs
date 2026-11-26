using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(SpriteRenderer), typeof(TemporalLightCharger))]
public class TemporalLightChargerVisuals : MonoBehaviour
{
    [SerializeField] private Color emptyColor = Color.gray;
    [SerializeField] private Color chargedColor = Color.yellow;
    [SerializeField] private string bloomStateName = "Bloom";

    private SpriteRenderer sr;
    private Animator animator;
    private TemporalLightCharger charger;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        charger = GetComponent<TemporalLightCharger>();
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 0f;
        }
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
        float normalizedTime = Mathf.Clamp(progressPercentage, 0f, 0.999f);
        if (animator != null)
        {
            animator.Play(bloomStateName, 0, normalizedTime);
        }
    }
}
