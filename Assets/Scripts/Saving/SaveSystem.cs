using System;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public static class SaveSystem
{
    public const int slotCount = 4;

    public static int ActiveSlot { get; private set; } = -1;
    public static SaveData Current { get; private set; }

    static string Folder => Path.Combine(Application.persistentDataPath, "Saves");
    static string FilePath(int slot) => Path.Combine(Folder, $"profile_{slot}.json");
    static string BackupPath(int slot) => FilePath(slot) + ".bak";
    static string TempPath(int slot) => FilePath(slot) + ".tmp";

    // Static fields can survive between Play session with certain EnterPlayMode options so this function resets them manually
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        ActiveSlot = -1;
        Current = null;
    }

    public static bool IsValidSlot(int slot) => slot >= 0 && slot < slotCount;

    public static bool SlotExists(int slot) =>
        IsValidSlot(slot) && (File.Exists(FilePath(slot)) || File.Exists(BackupPath(slot)));

    public static SaveData ReadSlot(int slot)
    {
        if (!IsValidSlot(slot)) return null;
        return TryRead(FilePath(slot)) ?? TryRead(BackupPath(slot));  // Try loading the save file, if it's null, try the backup
    }

    public static bool StartNewGame(int slot)
    {
        if (!IsValidSlot(slot)) return false;

        SaveData data = new SaveData();
        if (!Write(slot, data)) return false;

        ActiveSlot = slot;
        Current = data;
        return true;
    }

    public static bool LoadSlot(int slot)
    {
        SaveData data = ReadSlot(slot);
        if (data == null) return false;

        ActiveSlot = slot;
        Current = data;
        return true;
    }

    public static bool TrySaveAtPoint(int savePointId, AbilityType abilities)
    {
        if (Current == null) return false;                     // No active profile
        if (savePointId <= Current.savePointId) return false;  // Only save on newly discovered save points

        int oldId = Current.savePointId;
        int oldAbilities = Current.unlockedAbilities;

        Current.savePointId = savePointId;
        Current.unlockedAbilities = (int)abilities;

        if (Write(ActiveSlot, Current)) return true;

        // Writing failed so return to the old save
        Current.savePointId = oldId;
        Current.unlockedAbilities = oldAbilities;
        return false;
    }

    public static void DeleteSlot(int slot)
    {
        if (!IsValidSlot(slot)) return;

        try
        {
            // Delete the save file, the temp file and the backup
            foreach (string path in new[] { FilePath(slot), BackupPath(slot), TempPath(slot) })
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Coult not delete slot {slot}: {e.Message}");
        }

        if (slot == ActiveSlot)  // Reset the save slot to default
        {
            ActiveSlot = -1;
            Current = null;
        }
    }

    static SaveData TryRead(string path)
    {
        if (!File.Exists(path)) return null;

        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] Coult not read {path}: {e.Message}");
            return null;
        }
    }

    static bool Write(int slot, SaveData data)
    {
        try
        {
            Directory.CreateDirectory(Folder);  // Create save directory, if it doesn't exist

            string path = FilePath(slot);
            string temp = TempPath(slot);

            File.WriteAllText(temp, JsonUtility.ToJson(data, true));  // Write to temp file first

            if (File.Exists(path))
                File.Replace(temp, path, BackupPath(slot));  // Move data from the temp file to the save file and save the previous save as a backup
            else
                File.Move(temp, path);  // On the very first save, simply move data from temp to the save file since there is no prior save to backup

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Coult not write slot {slot}: {e.Message}");
            return false;
        }
    }
}
