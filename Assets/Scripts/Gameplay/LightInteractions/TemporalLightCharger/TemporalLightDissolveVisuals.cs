using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(TemporalLightCharger))]
public class TemporalLightDissolveVisuals : MonoBehaviour
{
    [Header("Dissolve Timing")]
    [SerializeField] private float dissolveAtFullCharge = 0.7f;
    [SerializeField] private float finishDissolveSpeed = 1f;
    [SerializeField] private float reformSpeed = 1.25f;

    [Header("Per-Object Colors")]
    [ColorUsage(true, true)]
    [SerializeField] private Color edgeColor = new Color(2.5f, 1.8f, 0.4f, 1f);

    private static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    private static readonly int DissolveColorID = Shader.PropertyToID("_DissolveColor");

    private SpriteRenderer sr;
    private TemporalLightCharger charger;
    private MaterialPropertyBlock propBlock;

    private bool isFullyCharged = false;
    private bool isReforming = false;
    private float currentDissolve = 0f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        charger = GetComponent<TemporalLightCharger>();
        propBlock = new MaterialPropertyBlock();

        ApplyProperties(0f);
    }

    private void OnEnable()
    {
        charger.OnChargeProgressAction += UpdateVisuals;
        charger.OnChargeStateChangedAction += HandleChargeStateChanged;
    }

    private void OnDisable()
    {
        charger.OnChargeProgressAction -= UpdateVisuals;
        charger.OnChargeStateChangedAction -= HandleChargeStateChanged;
    }

    private void Update()
    {
        if (isFullyCharged && currentDissolve < 1f)
        {
            currentDissolve = Mathf.MoveTowards(currentDissolve, 1f, finishDissolveSpeed * Time.deltaTime);
            ApplyProperties(currentDissolve);
        }
        else if (isReforming)
        {
            currentDissolve = Mathf.MoveTowards(currentDissolve, 0f, reformSpeed * Time.deltaTime);
            ApplyProperties(currentDissolve);

            if (currentDissolve <= 0f)
                isReforming = false;
        }
    }

    private void HandleChargeStateChanged(bool charged)
    {
        isFullyCharged = charged;
        isReforming = !charged;
    }

    private void UpdateVisuals(float progressPercentage)
    {
        if (isFullyCharged) return;

        float target = progressPercentage * dissolveAtFullCharge;
        if (isReforming && target < currentDissolve) return;

        isReforming = false;
        currentDissolve = target;
        ApplyProperties(currentDissolve);
    }

    private void ApplyProperties(float amount)
    {
        sr.GetPropertyBlock(propBlock);
        propBlock.SetFloat(DissolveAmountID, amount);
        propBlock.SetColor(DissolveColorID, edgeColor);
        sr.SetPropertyBlock(propBlock);
    }
}