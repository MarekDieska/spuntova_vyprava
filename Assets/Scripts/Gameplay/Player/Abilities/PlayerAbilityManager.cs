using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerAbilityManager : MonoBehaviour
{
    public static PlayerAbilityManager Instance { get; private set; }
    public event Action<AbilityType> OnAbilitiesUnlocked;
    [SerializeField] private AbilityType unlockedAbilities = AbilityType.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public bool IsUnlocked(AbilityType ability)
    {
        return (unlockedAbilities & ability) == ability;
    }
    public void Unlock(AbilityType abilitiesToUnlock)
    {
        AbilityType newUnlocked = abilitiesToUnlock & ~unlockedAbilities;
        if (newUnlocked == AbilityType.None) return;
        unlockedAbilities |= newUnlocked;
        Debug.Log($"[PlayerAbilityManager] Unlocked: {newUnlocked}");
        OnAbilitiesUnlocked?.Invoke(newUnlocked);
    }
    public void SetUnlockedAbilities(AbilityType abilities)
    {
        unlockedAbilities = abilities;
    }
    public AbilityType GetUnlockedAbilities() => unlockedAbilities;
}
