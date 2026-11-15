using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundChannelVolume : MonoBehaviour
{
    [SerializeField] SoundChannel channel = SoundChannel.Music;

    AudioSource source;
    float baseVolume;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        baseVolume = source.volume;
    }

    void OnEnable()
    {
        SoundSettings.Changed += Apply;
        Apply();
    }

    void OnDisable() => SoundSettings.Changed -= Apply;

    void Apply() => source.volume = baseVolume * SoundSettings.GetVolume(channel);
}
