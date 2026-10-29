using UnityEngine;

public class SectionTransition : MonoBehaviour
{
    public CameraController2D cameraController;
    public BoxCollider2D nextSectionBounds;
    public float duration = 0.7f;

    void OnTriggerEnter2D(Collider2D other)
    {
       cameraController.ChangeSection(nextSectionBounds, duration);
    }
}