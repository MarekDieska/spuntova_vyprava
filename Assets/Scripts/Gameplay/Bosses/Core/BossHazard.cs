using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossHazard : MonoBehaviour
{
    public event Action OnBossHit;
    public event Action OnHazardReformed;

    [Header("Hazard Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private bool autoReform = false;
    [SerializeField] private float reformDelay = 5f;
    [SerializeField] private TemporalLightCharger lightCharger;

    public float ReformDelay => reformDelay;
    public bool IsActive { get; private set; }
    public bool IsSpent { get; private set; }

    private Collider2D hazardCollider;

    private void Awake()
    {
        hazardCollider = GetComponent<Collider2D>();
        hazardCollider.isTrigger = true;

        if (lightCharger == null) lightCharger = GetComponent<TemporalLightCharger>();
        IsActive = false;
        IsSpent = false;
    }

    private void OnEnable()
    {
        if (lightCharger != null)
            lightCharger.OnChargeStateChangedAction += SetActive;
    }

    private void OnDisable()
    {
        if (lightCharger != null)
            lightCharger.OnChargeStateChangedAction -= SetActive;
    }

    public void SetActive(bool active)
    {
        if (IsSpent) return;
        IsActive = active;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageBoss(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamageBoss(other);
    }

    private void TryDamageBoss(Collider2D other)
    {
        if (!IsActive || IsSpent) return;

        BossBase boss = other.GetComponentInParent<BossBase>();
        if (boss == null || boss.IsDead) return;

        int hpBefore = boss.CurrentHp;
        boss.TakeDamage(damageAmount);

        if (boss.CurrentHp < hpBefore)
        {
            IsActive = false;
            IsSpent = true;
            lightCharger.ResetCharger();
            hazardCollider.enabled = false;
            OnBossHit?.Invoke();
            if (autoReform)
            {
                StartCoroutine(AutoReformRoutine());
            }
        }
    }
    private IEnumerator AutoReformRoutine()
    {
        yield return new WaitForSeconds(reformDelay);
        Reform();
    }
    public void Reform()
    {
        StopAllCoroutines();
        IsSpent = false;
        IsActive = false;

        lightCharger.ResetCharger();
        hazardCollider.enabled = true;

        OnHazardReformed?.Invoke();
    }
}