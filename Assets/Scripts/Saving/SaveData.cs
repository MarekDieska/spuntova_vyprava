using System;


[Serializable]
public class SaveData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public int savePointId = -1;
    public int unlockedAbilities = 0;
}
