using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(PlayerHealth))]
public class PlayerDamageVisuals : MonoBehaviour
{
    [Header("Invincibility Settings")]
    [SerializeField] private float iFrameDuration = 1.5f;
    [SerializeField] private float blinkInterval = 0.1f;

    private SpriteRenderer sr;
    private PlayerHealth health;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<PlayerHealth>();
    }

    private void OnEnable() => health.OnHealthChangedAction += HandleHealthChanged;
    private void OnDisable() => health.OnHealthChangedAction -= HandleHealthChanged;

    private void HandleHealthChanged(int currentHp, int maxHp, int amount)
    {
        if (amount < 0)
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        health.SetInvincible(true);

        float elapsed = 0f;
        while (elapsed < iFrameDuration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        sr.enabled = true;
        health.SetInvincible(false);
    }
}