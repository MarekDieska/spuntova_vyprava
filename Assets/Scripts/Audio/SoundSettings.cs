using System;
using UnityEngine;

public static class SoundSettings
{
    const string PrefsKeyPrefix = "Volume.";

    static readonly float[] volumes = new float[Enum.GetValues(typeof(SoundChannel)).Length];

    public static event Action Changed;

    public static float GetVolume(SoundChannel channel) => volumes[(int)channel];

    public static void SetVolume(SoundChannel channel, float volume)
    {
        volume = Mathf.Clamp01(volume);
        volumes[(int)channel] = volume;
        PlayerPrefs.SetFloat(PrefsKeyPrefix + channel, volume);

        if (channel == SoundChannel.Master)
            AudioListener.volume = volume;

        Changed?.Invoke();
    }

    public static void Save() => PlayerPrefs.Save();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Load()
    {
        Changed = null;

        foreach (SoundChannel channel in Enum.GetValues(typeof(SoundChannel)))
            volumes[(int)channel] = PlayerPrefs.GetFloat(PrefsKeyPrefix + channel, 1f);

        AudioListener.volume = GetVolume(SoundChannel.Master);
    }
}
