using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleAbility : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float regrabCooldown = 0.15f;

    [Header("Swinging")]
    [SerializeField] float swingForce = 25f;
    [SerializeField] float maxSwingSpeed = 12f;
    [SerializeField] float releaseBoost = 1.1f;

    public bool IsGrappled {get; private set; }

    PlayerInput input;
    PlayerMovement movement;

    Rigidbody2D rb;
    DistanceJoint2D joint;
    LineRenderer line;

    GrapplePoint current;
    GrapplePoint target;
    float cooldownTimer;
    float horizontalInput;
    bool grapplePressed, jumpPressed;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        joint = GetComponent<DistanceJoint2D>();
        line = GetComponent<LineRenderer>();

        joint.enabled = false;
        joint.autoConfigureConnectedAnchor = false;
        joint.autoConfigureDistance = false;
        joint.maxDistanceOnly = false;
        line.positionCount = 2;
        line.enabled = false;
    }

    private void Update()
    {
        horizontalInput = movement.MoveInput;
        grapplePressed |= input.GrapplePressedThisFrame;
        jumpPressed |= input.JumpPressedThisFrame;

        cooldownTimer -= Time.deltaTime;
        target = FindTarget();

        if (IsGrappled)
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, current.Position);
        }
    }

    private void FixedUpdate()
    {
        if (grapplePressed && target != null && cooldownTimer <= 0f)
        {
            Attach(target);
        }
        else if (jumpPressed && IsGrappled)
        {
            Release();
        }

        if (IsGrappled)
        {
            Swing();
        }

        grapplePressed = false;
        jumpPressed = false;
    }

    private GrapplePoint FindTarget()
    {
        GrapplePoint best = null;
        float bestDist = float.MaxValue;
        Vector2 pos = transform.position;

        foreach(var p in GrapplePoint.All)
        {
            if (p == current) continue;
            float d = Vector2.Distance(pos, p.Position);
            if (d > p.MaxRange || d >= bestDist) continue;
            if (Physics2D.Linecast(pos, p.Position, obstacleMask)) continue;

            best = p;
            bestDist = d;
        }
        return best;
    }

    private void Attach(GrapplePoint point)
    {
        current = point;
        joint.connectedAnchor = point.Position;
        joint.distance = point.RopeLength;
        joint.enabled = true;

        IsGrappled = true;
        line.enabled = true;
    }

    private void Swing()
    {
        Vector2 toPlayer = rb.position - current.Position;
        Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized;

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            float side = Mathf.Sign(Vector2.Dot(tangent, Vector2.right * horizontalInput));
            if (rb.linearVelocity.magnitude < maxSwingSpeed)
            {
                rb.AddForce(tangent * side * swingForce);
            }
        }
    }

    private void Release()
    {
        joint.enabled = false;
        IsGrappled = false;
        line.enabled = false;
        cooldownTimer = regrabCooldown;

        rb.linearVelocity *= releaseBoost;
        current = null;
    }
}