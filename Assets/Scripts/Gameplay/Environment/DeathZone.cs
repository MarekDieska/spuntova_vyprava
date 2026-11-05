
using UnityEngine;

public class DeathZone : MonoBehaviour
{

    [SerializeField] private Transform resetPosition;
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private AudioClip damageSound;

    private AudioSource damageAudio;

    private void Awake()
    {
        damageAudio = gameObject.AddComponent<AudioSource>();
        damageAudio.playOnAwake = false;
        damageAudio.spatialBlend = 0f;
        damageAudio.priority = 0;
        damageAudio.clip = damageSound;
        if (damageSound != null)
        {
            damageSound.LoadAudioData();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            if (damageSound != null)
            {
                damageAudio.Play();
            }
            damageable.TakeDamage(damageAmount);

            other.transform.position = resetPosition.position;
            FindFirstObjectByType<CameraController2D>()?.RespawnTransition();
        }
    }
}

