using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(LightTrigger))]
public class UpdateLightTriggerVisuals : MonoBehaviour
{
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

    private SpriteRenderer sr;
    private LightTrigger trigger;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        trigger = GetComponent<LightTrigger>();

        sr.sprite = offSprite;
    }

    private void OnEnable() => trigger.OnLightTriggerAction += HandleVisualsActivation;
    private void OnDisable() => trigger.OnLightTriggerAction -= HandleVisualsActivation;

    private void HandleVisualsActivation(bool isActive)
    {
        sr.sprite = isActive ? onSprite : offSprite;
    }
}
