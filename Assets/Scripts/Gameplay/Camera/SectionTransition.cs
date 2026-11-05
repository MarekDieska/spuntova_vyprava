using UnityEngine;

public class SectionTransition : MonoBehaviour
{
    public CameraController2D cameraController;
    public BoxCollider2D nextSectionBounds;
    public float duration = 0.7f;
    private bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!used && !other.isTrigger && other.TryGetComponent<PlayerMovement>(out _))
        {
            used = true;
            cameraController.ChangeSection(nextSectionBounds, duration);
            gameObject.SetActive(false);
        }
    }
}
