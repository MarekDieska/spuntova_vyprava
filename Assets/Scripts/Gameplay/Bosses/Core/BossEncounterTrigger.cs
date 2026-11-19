using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossEncounterTrigger : MonoBehaviour
{
    [Header("Boss References")]
    [SerializeField] private BossBase boss;
    [SerializeField] private BossHazardGroup hazardGroup;

    [Header("Encounter Timing")]
    [SerializeField] private float startDelay = 0.8f;

    [Header("Arena Gates")]
    [SerializeField] private GameObject entranceGate;
    [SerializeField] private GameObject exitGate;

    private Collider2D triggerCollider;
    private PlayerHealth trackedPlayerHealth;
    private Coroutine startCoroutine;
    private bool encounterRunning = false;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (entranceGate != null) entranceGate.SetActive(false);
        if (exitGate != null) exitGate.SetActive(true);
    }

    private void OnEnable()
    {
        boss.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        boss.OnBossDefeated -= HandleBossDefeated;

        UnsubscribePlayerDeath();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (encounterRunning || other.isTrigger) return;
        if (boss == null || boss.IsDead) return;

        if (other.TryGetComponent<PlayerMovement>(out _))
        {
            BeginEncounterSequence(other.gameObject);
        }
    }

    private void BeginEncounterSequence(GameObject playerObj)
    {
        encounterRunning = true;
        triggerCollider.enabled = false;

        if (entranceGate != null) entranceGate.SetActive(true);
        if (exitGate != null) exitGate.SetActive(true);

        if (playerObj.TryGetComponent(out trackedPlayerHealth))
        {
            trackedPlayerHealth.OnDeathAction += HandlePlayerDeath;
        }

        if (startCoroutine != null) StopCoroutine(startCoroutine);
        startCoroutine = StartCoroutine(DelayedStartRoutine());
    }

    private IEnumerator DelayedStartRoutine()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        if (encounterRunning && boss != null && !boss.IsDead)
        {
            boss.StartEncounter();
        }

        startCoroutine = null;
    }

    private void HandlePlayerDeath()
    {
        if (startCoroutine != null)
        {
            StopCoroutine(startCoroutine);
            startCoroutine = null;
        }

        UnsubscribePlayerDeath();
        encounterRunning = false;

        if (boss != null && !boss.IsDead)
        {
            boss.ResetBoss();
        }

        if (hazardGroup != null)
        {
            hazardGroup.ResetGroup();
        }

        if (entranceGate != null) entranceGate.SetActive(false);
        triggerCollider.enabled = true;
    }

    private void HandleBossDefeated()
    {
        if (startCoroutine != null)
        {
            StopCoroutine(startCoroutine);
            startCoroutine = null;
        }

        UnsubscribePlayerDeath();
        encounterRunning = false;

        if (entranceGate != null) entranceGate.SetActive(false);
        if (exitGate != null) exitGate.SetActive(false);
    }

    private void UnsubscribePlayerDeath()
    {
        if (trackedPlayerHealth != null)
        {
            trackedPlayerHealth.OnDeathAction -= HandlePlayerDeath;
            trackedPlayerHealth = null;
        }
    }
}