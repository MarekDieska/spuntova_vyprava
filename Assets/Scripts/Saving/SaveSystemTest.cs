using UnityEngine;

public class SaveSystemTest : MonoBehaviour
{
    [SerializeField] bool loadSlotsOnAwake = false;
    [SerializeField] int slot = 0;
    [SerializeField] int testId = 1;
    [SerializeField] AbilityType testAbilities = AbilityType.DoubleJump;

    [ContextMenu("New game")]
    void NewGame() => Debug.Log($"New game: {SaveSystem.StartNewGame(slot)}");

    [ContextMenu("Load slot")]
    void Load() => Debug.Log($"Load: {SaveSystem.LoadSlot(slot)}");

    [ContextMenu("Try save at test ID")]
    void TrySave() => Debug.Log($"Save at {testId}: {SaveSystem.TrySaveAtPoint(testId, testAbilities)}");

    [ContextMenu("Log current")]
    void LogCurrent()
    {
        SaveData c = SaveSystem.Current;
        Debug.Log(c == null ? "No active profile"
            : $"Active slot {SaveSystem.ActiveSlot}: id {c.savePointId}, abilities {(AbilityType)c.unlockedAbilities}");
    }

    [ContextMenu("Delete slot")]
    void Delete()
    {
        SaveSystem.DeleteSlot(slot);
        Debug.Log($"Deleted slot {slot}");
    }

    [ContextMenu("Log save folder")]
    void LogFolder() => Debug.Log(Application.persistentDataPath);

    void Awake()
    {
        if (loadSlotsOnAwake) SaveSystem.LoadSlot(slot);
    }
}
