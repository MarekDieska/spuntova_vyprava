using UnityEngine;

[DefaultExecutionOrder(100)]
public class SaveLoader : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField] private CameraController2D cameraController;

    private void Start()
    {
        if (player == null || cameraController == null)
        {
            Debug.LogError("[SaveLoader] Player or camera reference is missing.", this);
            return;
        }

        SaveData data = SaveSystem.Current;
        if (data == null || data.savePointId < 0) return;  // No profile or nothing saved yet

        SectionTransition point = FindSavePoint(data.savePointId);
        if (point == null)
        {
            Debug.LogWarning($"[SaveLoader] No save point with ID {data.savePointId} in this scene.");
            return;
        }

        if (PlayerAbilityManager.Instance != null)
        {
            PlayerAbilityManager.Instance.SetUnlockedAbilities((AbilityType)data.unlockedAbilities);
        }

        player.SetRespawnPoint(point.RespawnPosition);
        player.transform.position = point.RespawnPosition;
        Physics2D.SyncTransforms();

        cameraController.ChangeSection(point.NextSectionBounds, 0f);
        cameraController.Snap();

        point.gameObject.SetActive(false);
    }

    private static SectionTransition FindSavePoint(int id)
    {
        foreach (SectionTransition t in SectionTransition.All)
        {
            if (t.IsSavePoint && t.SavePointId == id) return t;
        }
        return null;
    }
}
