using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField] AudioClip footstepSound;
    [SerializeField, Range(0f, 1f)] float volume = 0.5f;
    [SerializeField, Min(0.1f)] float interval = 0.28f;

    [Header("Landing")]
    [SerializeField] AudioClip landingSound;
    [SerializeField, Range(0f, 1f)] float landingVolume = 0.7f;
    [SerializeField, Min(0f)] float minLandingSpeed = 2f;

    const float MinPitch = 0.82f;
    const float MaxPitch = 1.04f;
    const float FadeOutTime = 0.08f;

    PlayerMovement movement;
    AudioSource stepSource;
    AudioSource landingSource;
    float nextStepTime;

    static float PlayerVolume => SoundSettings.GetVolume(SoundChannel.Player);

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        stepSource = CreateSource(footstepSound);
        landingSource = CreateSource(landingSound);
    }

    void OnEnable() => movement.Landed += PlayLanding;

    void OnDisable() => movement.Landed -= PlayLanding;

    void LateUpdate()
    {
        if (footstepSound == null || Time.timeScale <= 0f || !movement.IsWalking)
        {
            FadeOutStep();
            return;
        }

        stepSource.volume = volume * PlayerVolume;
        if (Time.time < nextStepTime)
            return;

        stepSource.pitch = Random.Range(MinPitch, MaxPitch);
        stepSource.Play();
        nextStepTime = Time.time + interval;
    }

    void PlayLanding(float impactSpeed)
    {
        if (landingSound == null || impactSpeed < minLandingSpeed)
            return;

        landingSource.PlayOneShot(landingSound, landingVolume * PlayerVolume);
        nextStepTime = Time.time + interval;
    }

    void FadeOutStep()
    {
        if (!stepSource.isPlaying)
            return;

        stepSource.volume = Mathf.MoveTowards(stepSource.volume, 0f, volume * Time.unscaledDeltaTime / FadeOutTime);
        if (stepSource.volume <= 0f)
            stepSource.Stop();
    }

    AudioSource CreateSource(AudioClip clip)
    {
        AudioSource created = gameObject.AddComponent<AudioSource>();
        created.playOnAwake = false;
        created.clip = clip;

        if (clip != null)
            clip.LoadAudioData();

        return created;
    }
}