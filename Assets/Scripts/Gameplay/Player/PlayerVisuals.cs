using UnityEngine;

[RequireComponent(typeof(PlayerInput)), RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisuals : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Sprite spunt_test_default;
    [SerializeField] private Sprite spunt_test_left;
    [SerializeField] private Sprite spunt_test_right;

    private SpriteRenderer current_sprite;
    private PlayerInput input;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        current_sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        UpdateSprites(input.MoveInput);
    }

    private void UpdateSprites(float moveInput)
    {
        const float StickDeadZone = 0.3f;

        if (Mathf.Abs(moveInput) < StickDeadZone)
        {
            current_sprite.sprite = spunt_test_default;
        }
        else if (moveInput < 0f)
        {
            current_sprite.sprite = spunt_test_left;
        }
        else
        {
            current_sprite.sprite = spunt_test_right;
        }
    }
}