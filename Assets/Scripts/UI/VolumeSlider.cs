using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    [SerializeField] SoundChannel channel;

    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnEnable() => slider.SetValueWithoutNotify(SoundSettings.GetVolume(channel));

    void OnSliderChanged(float value) => SoundSettings.SetVolume(channel, value);
}
