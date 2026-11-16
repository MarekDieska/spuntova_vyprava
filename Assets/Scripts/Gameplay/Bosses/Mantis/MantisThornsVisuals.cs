using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BossHazard), typeof(TemporalLightCharger))]
public class MantisThornVisuals : MonoBehaviour
{
    [Header("Dissolve & Reform Timing")]
    [SerializeField] private float dissolveSpeed = 1.25f;
    [SerializeField] private float reformDuration = 1.5f;

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

    private Coroutine dissolveRoutine;
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
        charger.OnChargeProgressAction += HandleChargeProgress;
    }

    private void OnDisable()
    {
        hazard.OnBossHit -= HandleBossHit;
        charger.OnChargeProgressAction -= HandleChargeProgress;
    }

    private void HandleChargeProgress(float progress)
    {
        if (isDissolvedState) return;

        currentDissolve = progress * chargedDissolveShimmer;
        currentEdgeColor = Color.Lerp(inactiveEdgeColor, activeEdgeColor, progress);
        ApplyProperties(currentDissolve, currentEdgeColor);
    }

    [ContextMenu("Boss hit")]//DEBUG
    public void HandleBossHit()
    {
        if (dissolveRoutine != null) StopCoroutine(dissolveRoutine);
        dissolveRoutine = StartCoroutine(DissolveAndReformRoutine());
    }

    private IEnumerator DissolveAndReformRoutine()
    {
        isDissolvedState = true;
        float totalReformTime = hazard.ReformDelay;
        float elapsed = 0f;
        currentDissolve = 0f;

        while (currentDissolve < 1f)
        {
            float dt = Time.deltaTime;
            elapsed += dt;
            currentDissolve = Mathf.MoveTowards(currentDissolve, 1f, dissolveSpeed * dt);
            ApplyProperties(currentDissolve, activeEdgeColor);
            yield return null;
        }

        float waitDuration = Mathf.Max(0f, totalReformTime - elapsed - reformDuration);
        if (waitDuration > 0f)
        {
            yield return new WaitForSeconds(waitDuration);
        }

        float actualReformTime = Mathf.Max(0.01f, Mathf.Min(reformDuration, totalReformTime - elapsed));
        float reformSpeed = 1f / actualReformTime;

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
        dissolveRoutine = null;
    }

    private void ApplyProperties(float amount, Color color)
    {
        spriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(DissolveAmountID, amount);
        propBlock.SetColor(DissolveColorID, color);
        spriteRenderer.SetPropertyBlock(propBlock);
    }
}