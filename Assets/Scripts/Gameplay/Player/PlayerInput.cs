using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Movement Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference dashAction;
    [Header("Light Input")]
    [SerializeField] private InputActionReference lightAction;
    [SerializeField] private InputActionReference aimPositionAction;
    [SerializeField] private InputActionReference lightToggleAction;
    [SerializeField] private InputActionReference grappleAction;

    public float MoveInput { get; private set; }
    public float VerticalInput { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool DashPressedThisFrame { get; private set; }
    public bool LightHeld { get; private set; }
    public bool LightTogglePressedThisFrame { get; private set; }
    public Vector2 MouseScreenPosition { get; private set; }
    public bool GrapplePressedThisFrame { get; private set; }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
        if (dashAction != null) dashAction.action.Enable();
        if (lightAction != null) lightAction.action.Enable();
        if (aimPositionAction != null) aimPositionAction.action.Enable();
        if (lightToggleAction != null) lightToggleAction.action.Enable();
        if (grappleAction != null) grappleAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
        if (dashAction != null) dashAction.action.Disable();
        if (lightAction != null) lightAction.action.Disable();
        if (aimPositionAction != null) aimPositionAction.action.Disable();
        if (lightToggleAction != null) lightToggleAction.action.Disable();
        if (grappleAction != null) grappleAction.action.Disable();
    }

    private void Update()
    {
        Vector2 move = moveAction.action.ReadValue<Vector2>();
        MoveInput = move.x;
        VerticalInput = move.y;
        JumpHeld = jumpAction.action.IsPressed();
        JumpPressedThisFrame = jumpAction.action.WasPressedThisFrame();
        DashPressedThisFrame = dashAction.action.WasPressedThisFrame();
        LightHeld = lightAction.action.IsPressed();
        LightTogglePressedThisFrame = lightToggleAction.action.WasPressedThisFrame();
        MouseScreenPosition = aimPositionAction.action.ReadValue<Vector2>();
        GrapplePressedThisFrame = grappleAction.action.WasPerformedThisFrame();
    }
}