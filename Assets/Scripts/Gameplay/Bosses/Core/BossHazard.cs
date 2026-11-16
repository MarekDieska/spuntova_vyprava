using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossHazard : MonoBehaviour
{
    public event Action OnBossHit;

    [Header("Hazard Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float reformDelay = 5f;
    [SerializeField] private TemporalLightCharger lightCharger;

    public float ReformDelay => reformDelay;
    public bool IsActive { get; private set; }

    private Collider2D hazardCollider;
    private bool isReforming = false;

    private void Awake()
    {
        hazardCollider = GetComponent<Collider2D>();
        hazardCollider.isTrigger = true;

        if (lightCharger == null) lightCharger = GetComponent<TemporalLightCharger>();
        IsActive = false;
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
        if (isReforming) return;
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
        if (!IsActive || isReforming) return;

        BossBase boss = other.GetComponentInParent<BossBase>();
        if (boss == null || boss.IsDead) return;

        int hpBefore = boss.CurrentHp;
        boss.TakeDamage(damageAmount);

        if (boss.CurrentHp < hpBefore)
        {
            IsActive = false;
            OnBossHit?.Invoke();
            StartCoroutine(ReformRoutine());
        }
    }

    private IEnumerator ReformRoutine()
    {
        isReforming = true;

        lightCharger.ResetCharger();
        hazardCollider.enabled = false;

        yield return new WaitForSeconds(reformDelay);

        hazardCollider.enabled = true;
        isReforming = false;
    }
}