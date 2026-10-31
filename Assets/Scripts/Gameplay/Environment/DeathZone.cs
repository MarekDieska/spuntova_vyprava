
using UnityEngine;

public class DeathZone : MonoBehaviour
{

    [SerializeField] private Transform resetPosition;
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damageAmount);

            other.transform.position = resetPosition.position;
        }
    }
}

