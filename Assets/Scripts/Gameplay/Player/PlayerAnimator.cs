using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private PlayerInput playerInput;
    private ConeLightAbility coneLight;
    private MovableLightAbility movableLight;
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsLightHash = Animator.StringToHash("IsLight");
    private float currentFacingDirection = 1f;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerInput = GetComponentInParent<PlayerInput>();
        coneLight = GetComponentInParent<ConeLightAbility>();
        movableLight = GetComponentInParent<MovableLightAbility>();
    }

    private void Update()
    {
        UpdateAnimationParameters();
    }

    private void LateUpdate()
    {
        HandleVisualFlipping();
    }

    private void UpdateAnimationParameters()
    {
        if (animator == null) return;

        bool isMoving = false;
        if (playerMovement != null)
        {
            isMoving = Mathf.Abs(playerMovement.MoveInput) > 0.01f && playerMovement.IsGrounded;
        }

        bool isLightActive = false;
        if (playerInput != null)
        {
            isLightActive = playerInput.LightHeld;

        }

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetBool(IsLightHash, isLightActive);
    }
    private void HandleVisualFlipping()
    {
        float facingInput = playerMovement.MoveInput;
        if (coneLight != null && coneLight.IsShining)
        {
            facingInput = coneLight.AimDirection.normalized.x;
        }
        else if (movableLight != null && movableLight.IsShining)
        {
            facingInput = movableLight.AimDirection.normalized.x;
        }

        if (facingInput > 0.01f && currentFacingDirection != 1f)
        {
            SetFacingDirection(1f);
        }
        else if (facingInput < -0.01f && currentFacingDirection != -1f)
        {
            SetFacingDirection(-1f);
        }
    }
    private void SetFacingDirection(float direction)
    {
        currentFacingDirection = direction;

        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * direction;
        transform.localScale = localScale;
    }
}
