using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public event Action<int, int, int> OnHealthChangedAction; //<current, max, deltaChange>
    public event Action OnDeathAction;
    [SerializeField] private int maxHp = 3;

    private int currentHp;
    private bool isInvincible = false;

    private void Awake()
    {
        currentHp = maxHp;
    }
    private void Start()
    {
        OnHealthChangedAction?.Invoke(currentHp, maxHp, 0);
    }
    public void TakeDamage(int amount)
    {
        if (isInvincible || currentHp <= 0) return;

        currentHp = Mathf.Max(0, currentHp - amount);

        OnHealthChangedAction?.Invoke(currentHp, maxHp, amount);
        if (currentHp == 0)
        {
            OnDeathAction?.Invoke();
        }
    }
    public void SetInvincible(bool state)
    {
        isInvincible = state;
    }

    [SerializeField] private int hpAdjust = 1;
    //SIMULATE DIFFUCULTY CHANGE IF WORKS
    [ContextMenu("Trigger Max Health Adjustment")]
    public void ChangeHealth()
    {
        maxHp = hpAdjust;
        currentHp = maxHp;
        OnHealthChangedAction?.Invoke(currentHp, maxHp, 0);
    }
}
