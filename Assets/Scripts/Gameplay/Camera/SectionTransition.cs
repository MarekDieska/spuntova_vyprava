using System.Collections.Generic;
using UnityEngine;

public class SectionTransition : MonoBehaviour
{
    public static readonly List<SectionTransition> All = new List<SectionTransition>();

    [SerializeField] private CameraController2D cameraController;
    [SerializeField] private BoxCollider2D nextSectionBounds;
    [SerializeField] private float duration = 0.7f;
    [SerializeField] private Transform nextRoomRespawnPoint;

    [Header("Save Point")]
    [SerializeField] private bool isSavePoint = false;
    [SerializeField, Min(0)] private int savePointId;

    private bool used;

    public bool IsSavePoint => isSavePoint;
    public int SavePointId => savePointId;
    public Vector2 RespawnPosition => nextRoomRespawnPoint.position;
    public BoxCollider2D NextSectionBounds => nextSectionBounds;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => All.Clear();

    private void Awake() => All.Add(this);
    private void OnDestroy() => All.Remove(this);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!used && !other.isTrigger && other.TryGetComponent<PlayerMovement>(out _))
        {
            used = true;
            if (other.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
            {
                playerHealth.SetRespawnPoint(nextRoomRespawnPoint.position);
            }
            TrySave();
            cameraController.ChangeSection(nextSectionBounds, duration);
            gameObject.SetActive(false);
        }
    }

    private void TrySave()
    {
        if (!isSavePoint) return;

        AbilityType abilites = PlayerAbilityManager.Instance != null 
            ? PlayerAbilityManager.Instance.GetUnlockedAbilities()
            : AbilityType.None;

        if (SaveSystem.TrySaveAtPoint(savePointId, abilites))
        {
            Debug.Log($"[SectionTransition] Saved at point {savePointId}");
        }
    }
}
