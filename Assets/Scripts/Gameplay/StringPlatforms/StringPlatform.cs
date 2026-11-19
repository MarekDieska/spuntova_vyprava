using UnityEngine;

public class StringPlatform : MonoBehaviour
{
    [SerializeField] private float bounceHeight = 6f;
    [SerializeField] private float minTopNormal = 0.5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.TryGetComponent(out PlayerMovement player))
        {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y <= -minTopNormal)
            {
                player.Bounce(bounceHeight);
                return;
            }
        }
    }
}
