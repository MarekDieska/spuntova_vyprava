using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BossHazard), typeof(TemporalLightCharger))]
public class MantisThornVisuals : MonoBehaviour
{
    [Header("Dissolve & Reform Timing")]
    [SerializeField] private float dissolveSpeed = 1.25f;
    [SerializeField] private float reformSpeed = 1.25f;

    [Header("Charge & Active Visuals")]
    [SerializeField] private float chargedDissolveShimmer = 0.2f;
    [ColorUsage(true, true)]
    [SerializeField] private Color inactiveEdgeColor = Color.black;
    [ColorUsage(true, true)]
    [SerializeField] private Color activeEdgeColor = new Color(2.5f, 1.8f, 0.4f, 1f);

    private static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    private static readonly int DissolveColorID = Shader.PropertyToID("_DissolveColor");

    private SpriteRenderer spriteRenderer;
    private BossHazard hazard;
    private TemporalLightCharger charger;
    private MaterialPropertyBlock propBlock;

    private Coroutine transitionRoutine;
    private bool isDissolvedState = false;
    private float currentDissolve = 0f;
    private Color currentEdgeColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        hazard = GetComponent<BossHazard>();
        charger = GetComponent<TemporalLightCharger>();
        propBlock = new MaterialPropertyBlock();

        currentEdgeColor = inactiveEdgeColor;
        ApplyProperties(0f, currentEdgeColor);
    }

    private void OnEnable()
    {
        hazard.OnBossHit += HandleBossHit;
        hazard.OnHazardReformed += HandleHazardReformed;
        charger.OnChargeProgressAction += HandleChargeProgress;
    }

    private void OnDisable()
    {
        hazard.OnBossHit -= HandleBossHit;
        hazard.OnHazardReformed -= HandleHazardReformed;
        charger.OnChargeProgressAction -= HandleChargeProgress;
    }

    private void HandleChargeProgress(float progress)
    {
        if (isDissolvedState) return;

        currentDissolve = progress * chargedDissolveShimmer;
        currentEdgeColor = Color.Lerp(inactiveEdgeColor, activeEdgeColor, progress);
        ApplyProperties(currentDissolve, currentEdgeColor);
    }

    public void HandleBossHit()
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(DissolveRoutine());
    }
    public void HandleHazardReformed()
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(ReformRoutine());
    }
    private IEnumerator DissolveRoutine()
    {
        isDissolvedState = true;

        while (currentDissolve < 1f)
        {
            currentDissolve = Mathf.MoveTowards(currentDissolve, 1f, dissolveSpeed * Time.deltaTime);
            ApplyProperties(currentDissolve, activeEdgeColor);
            yield return null;
        }

        transitionRoutine = null;
    }
    private IEnumerator ReformRoutine()
    {
        isDissolvedState = true;

        while (currentDissolve > 0f)
        {
            currentDissolve = Mathf.MoveTowards(currentDissolve, 0f, reformSpeed * Time.deltaTime);
            Color color = Color.Lerp(inactiveEdgeColor, activeEdgeColor, currentDissolve);
            ApplyProperties(currentDissolve, color);
            yield return null;
        }

        currentEdgeColor = inactiveEdgeColor;
        ApplyProperties(0f, currentEdgeColor);
        isDissolvedState = false;
        transitionRoutine = null;
    }

    private void ApplyProperties(float amount, Color color)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (propBlock == null) propBlock = new MaterialPropertyBlock();

        spriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(DissolveAmountID, amount);
        propBlock.SetColor(DissolveColorID, color);
        spriteRenderer.SetPropertyBlock(propBlock);
    }
}