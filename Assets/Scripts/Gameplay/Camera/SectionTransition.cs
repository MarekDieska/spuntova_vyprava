using UnityEngine;

public class SectionTransition : MonoBehaviour
{
    [SerializeField] private CameraController2D cameraController;
    [SerializeField] private BoxCollider2D nextSectionBounds;
    [SerializeField] private float duration = 0.7f;
    [SerializeField] private Transform nextRoomRespawnPoint;
    private bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!used && !other.isTrigger && other.TryGetComponent<PlayerMovement>(out _))
        {
            used = true;
            if (other.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
            {
                playerHealth.SetRespawnPoint(nextRoomRespawnPoint.position);
            }
            cameraController.ChangeSection(nextSectionBounds, duration);
            gameObject.SetActive(false);
        }
    }
}
