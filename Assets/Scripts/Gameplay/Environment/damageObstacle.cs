using UnityEngine;

public class damageObstacle : MonoBehaviour
{
    [SerializeField] private Transform resetPosition;
    [SerializeField] private int damageAmount = 1;

    private void Awake()
    {
        if (resetPosition == null)
        {
            resetPosition = transform.Find("ResetPosition");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RespawnPlayer(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        RespawnPlayer(collision.collider);
    }

    private void RespawnPlayer(Collider2D other)
    {
        if (resetPosition != null && other.TryGetComponent(out PlayerHealth player))
        {
            player.TakeDamage(damageAmount);
            other.transform.position = resetPosition.position;
            FindFirstObjectByType<CameraController2D>()?.RespawnTransition();
        }
    }
}
