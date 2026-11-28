using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class LeafProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 4f;

    private GameObject owner;

    public void Initialize(Vector2 direction, GameObject ownerObject)
    {
        owner = ownerObject;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner || other.isTrigger) return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }

    }
}