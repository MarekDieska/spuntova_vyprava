using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Movement Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [Header("Light Input")]
    [SerializeField] private InputActionReference lightAction;
    [SerializeField] private InputActionReference aimPositionAction;
    [SerializeField] private InputActionReference grappleAction;


    public float MoveInput { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool LightHeld { get; private set; }
    public Vector2 MouseScreenPosition { get; private set; }
    public bool GrapplePressedThisFrame { get; private set; }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
        if (lightAction != null) lightAction.action.Enable();
        if (aimPositionAction != null) aimPositionAction.action.Enable();
        if (grappleAction != null) grappleAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
        if (lightAction != null) lightAction.action.Disable();
        if (aimPositionAction != null) aimPositionAction.action.Disable();
        if (grappleAction != null) grappleAction.action.Disable();
    }

    private void Update()
    {
        MoveInput = moveAction.action.ReadValue<Vector2>().x;
        JumpHeld = jumpAction.action.IsPressed();
        JumpPressedThisFrame = jumpAction.action.WasPressedThisFrame();
        LightHeld = lightAction.action.IsPressed();
        MouseScreenPosition = aimPositionAction.action.ReadValue<Vector2>();
        GrapplePressedThisFrame = grappleAction.action.WasPerformedThisFrame();
    }
}