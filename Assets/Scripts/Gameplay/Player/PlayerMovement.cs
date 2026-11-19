using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    public enum PlayerState { Normal, Grappled }

    [Header("Collision")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField, Range(0f, 89f)] float maxSlopeAngle = 50f;

    [Header("Run")]
    [SerializeField] float maxSpeed = 8f;
    [SerializeField] float acceleration = 90f;
    [SerializeField, Range(0f, 1f)] float airControl = 0.65f;

    [Header("Jump")]
    [SerializeField] float jumpHeight = 3.2f;
    [SerializeField] float timeToApex = 0.4f;
    [SerializeField] float fallGravityMultiplier = 2f;
    [SerializeField, Range(0f, 1f)] float jumpCutMultiplier = 0.5f;
    [SerializeField] float maxFallSpeed = 18f;

    [Header("Air Jump")]
    [SerializeField] int airJumps = 1;
    [SerializeField] float airJumpHeight = 2.5f;

    [Header("Wall")]
    [SerializeField] float wallSlideSpeed = 3f;
    [SerializeField] float wallJumpPushSpeed = 11f;
    [SerializeField] float wallJumpControlTime = 0.2f;

    [Header("Assists")]
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBufferTime = 0.1f;
    [SerializeField] float cornerCorrection = 0.25f;
    [SerializeField] float maxStepHeight = 0.15f;

    const float ContactDistance = 0.05f;
    const float SnapDistance = 0.3f;
    const float JumpGroundIgnoreTime = 0.1f;
    const float WallJumpDistance = 0.25f;
    const float WallCheckHeight = 0.6f;
    const float WallNormalThreshold = 0.9f;
    const float Skin = 0.02f;
    const float GroundStickSpeed = 1f;
    const float ApexSpeed = 2f;
    const float ApexGravityMultiplier = 0.5f;
    const float CornerStep = 0.02f;
    const float StickDeadZone = 0.3f;
    const float WalkSpeedThreshold = 0.1f;
    const float DropInputThreshold = 0.5f;
    const float DropThroughTime = 0.2f;

    readonly List<RaycastHit2D> castHits = new List<RaycastHit2D>();
    readonly List<Collider2D> overlapHits = new List<Collider2D>();
    readonly List<Collider2D> droppedPlatforms = new List<Collider2D>();
    readonly ContactFilter2D solidFilter = new ContactFilter2D { useTriggers = false };

    Rigidbody2D rb;
    BoxCollider2D col;
    PlayerInput input;

    Vector2 velocity;
    Vector2 carriedVelocity;
    Vector2 groundVelocity;
    Vector2 groundNormal = Vector2.up;
    float moveInput;
    float fallSpeed;
    bool jumpHeld;
    bool dropHeld;
    bool isGrounded;
    bool isWallSliding;
    bool canCutJump;
    int airJumpsLeft;
    float leftBoundary = float.NegativeInfinity;
    float lastJumpPressedTime = float.NegativeInfinity;
    float lastGroundedTime = float.NegativeInfinity;
    float lastJumpTime = float.NegativeInfinity;
    float wallJumpTime = float.NegativeInfinity;
    float lastDropTime = float.NegativeInfinity;

    public event System.Action<float> Landed;

    public PlayerState State { get; set; }
    public float MoveInput => moveInput;
    public bool IsGrounded => isGrounded;
    public bool IsWallSliding => isWallSliding;
    public Vector2 Velocity => velocity;

    public bool IsWalking =>
        isActiveAndEnabled && isGrounded && moveInput != 0f && State == PlayerState.Normal &&
        (rb.constraints & RigidbodyConstraints2D.FreezePositionX) == 0 &&
        Mathf.Abs(rb.linearVelocity.x - carriedVelocity.x) > WalkSpeedThreshold;

    float Gravity => 2f * jumpHeight / (timeToApex * timeToApex);

    void Reset() => groundLayer = LayerMask.GetMask("Ground");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        input = GetComponent<PlayerInput>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        col.sharedMaterial = new PhysicsMaterial2D { friction = 0f };

        airJumpsLeft = airJumps;
    }

    void Update()
    {
        moveInput = Mathf.Abs(input.MoveInput) < StickDeadZone ? 0f : Mathf.Sign(input.MoveInput);
        jumpHeld = input.JumpHeld;
        dropHeld = input.VerticalInput < -DropInputThreshold;

        if (input.JumpPressedThisFrame)
            lastJumpPressedTime = Time.time;
    }

    void FixedUpdate()
    {
        velocity = rb.linearVelocity - carriedVelocity;

        if (State == PlayerState.Normal)
        {
            Run();
            StepOverBumps();
            CutJumpIfReleased();
            CorrectCorners();
        }

        RestoreDroppedPlatforms();
        CheckSurroundings();
        TryJump();
        ApplyGravity();
        fallSpeed = isGrounded ? 0f : Mathf.Max(0f, -velocity.y);

        carriedVelocity = isGrounded ? groundVelocity : Vector2.zero;
        rb.linearVelocity = velocity + carriedVelocity;
        ApplyLeftBoundary();
    }

    public void SetLeftBoundary(float x)
    {
        leftBoundary = x;
        ApplyLeftBoundary();
    }

    public void Bounce(float height)
    {
        if (State != PlayerState.Normal)
            return;

        velocity = rb.linearVelocity - carriedVelocity;
        Jump(height);
        canCutJump = false;
        airJumpsLeft = airJumps;
        carriedVelocity = Vector2.zero;
        rb.linearVelocity = velocity;
    }

    void ApplyLeftBoundary()
    {
        if (float.IsNegativeInfinity(leftBoundary))
            return;

        float minX = leftBoundary + col.bounds.extents.x - transform.TransformVector(col.offset).x;
        Vector2 position = rb.position;
        position.x = Mathf.Max(position.x, minX);
        rb.position = position;

        Vector2 currentVelocity = rb.linearVelocity;
        currentVelocity.x = Mathf.Max(currentVelocity.x, (minX - position.x) / Time.fixedDeltaTime);
        rb.linearVelocity = currentVelocity;
    }

    void CheckSurroundings()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;
        groundNormal = Vector2.up;
        groundVelocity = Vector2.zero;

        bool justJumped = Time.time - lastJumpTime < JumpGroundIgnoreTime;
        float groundDistance = wasGrounded ? SnapDistance : ContactDistance;
        bool canLandOnOneWay = velocity.y <= 0f || (wasGrounded && State == PlayerState.Normal);
        RaycastHit2D ground = Cast(Vector2.down, groundDistance, 1f, canLandOnOneWay);
        if (ground && dropHeld && State == PlayerState.Normal && IsOneWay(ground.collider))
        {
            DropThrough(ground.collider);
            ground = Cast(Vector2.down, groundDistance, 1f, canLandOnOneWay);
        }

        if (ground && !justJumped && ground.normal.y >= Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad))
        {
            isGrounded = true;
            groundNormal = ground.normal;
            groundVelocity = PlatformVelocity(ground.collider);
            lastGroundedTime = Time.time;

            float gap = ground.distance - Skin;
            if (gap > Skin)
                rb.position += Vector2.down * (gap - Skin * 0.5f);

            if (!wasGrounded)
                Landed?.Invoke(fallSpeed);
        }

        int wall = isGrounded ? 0 : WallSide(ContactDistance);
        isWallSliding = wall != 0 && moveInput == wall && velocity.y <= 0f;

        if (State == PlayerState.Grappled)
            isGrounded = true;

        if (isGrounded || isWallSliding)
            airJumpsLeft = airJumps;
    }

    void DropThrough(Collider2D platform)
    {
        Physics2D.IgnoreCollision(col, platform, true);
        droppedPlatforms.Add(platform);
        lastDropTime = Time.time;
        lastGroundedTime = float.NegativeInfinity;
    }

    void RestoreDroppedPlatforms()
    {
        if (Time.time - lastDropTime < DropThroughTime)
            return;

        for (int i = droppedPlatforms.Count - 1; i >= 0; i--)
        {
            Collider2D platform = droppedPlatforms[i];
            if (platform == null)
            {
                droppedPlatforms.RemoveAt(i);
            }
            else if (!col.Distance(platform).isOverlapped)
            {
                Physics2D.IgnoreCollision(col, platform, false);
                droppedPlatforms.RemoveAt(i);
            }
        }
    }

    void Run()
    {
        float rate = isGrounded ? acceleration : acceleration * airControl;

        float sinceWallJump = Time.time - wallJumpTime;
        if (sinceWallJump < wallJumpControlTime)
            rate *= sinceWallJump / wallJumpControlTime;

        float step = rate * Time.fixedDeltaTime;
        if (isGrounded)
        {
            Vector2 along = new Vector2(groundNormal.y, -groundNormal.x);
            float speed = Mathf.MoveTowards(Vector2.Dot(velocity, along), moveInput * maxSpeed, step);
            velocity = along * speed;
        }
        else
        {
            velocity.x = Mathf.MoveTowards(velocity.x, moveInput * maxSpeed, step);
        }
    }

    void StepOverBumps()
    {
        if (!isGrounded || moveInput == 0f || maxStepHeight <= 0f)
            return;

        Bounds b = col.bounds;
        Vector2 direction = new Vector2(moveInput, 0f);
        float reach = Mathf.Abs(velocity.x) * Time.fixedDeltaTime + ContactDistance;

        Vector2 feetSize = new Vector2(b.size.x - 2f * Skin, maxStepHeight);
        Vector2 feetCenter = new Vector2(b.center.x, b.min.y + Skin + maxStepHeight * 0.5f);
        RaycastHit2D hit = CastFrom(feetCenter, feetSize, direction, reach);
        if (!hit || Mathf.Abs(hit.normal.x) < WallNormalThreshold)
            return;

        Vector2 bodySize = new Vector2(b.size.x - 2f * Skin, b.size.y - 2f * Skin);
        Vector2 ahead = (Vector2)b.center + direction * reach;
        for (float lift = CornerStep; lift <= maxStepHeight + CornerStep; lift += CornerStep)
        {
            if (!Overlaps(ahead + Vector2.up * lift, bodySize))
            {
                rb.position += Vector2.up * lift;
                return;
            }
        }
    }

    void CorrectCorners()
    {
        if (velocity.y <= 0f || isGrounded)
            return;

        Bounds b = col.bounds;
        float rise = velocity.y * Time.fixedDeltaTime;

        Vector2 size = new Vector2(b.size.x - 2f * Skin, b.size.y - 2f * Skin);
        Vector2 center = (Vector2)b.center + Vector2.up * (rise + Skin);
        if (!Overlaps(center, size))
            return;

        for (float shift = CornerStep; shift <= cornerCorrection; shift += CornerStep)
        {
            if (velocity.x >= 0f && !Overlaps(center + Vector2.right * shift, size))
            {
                rb.position += Vector2.right * shift;
                return;
            }
            if (velocity.x <= 0f && !Overlaps(center + Vector2.left * shift, size))
            {
                rb.position += Vector2.left * shift;
                return;
            }
        }
    }

    void CutJumpIfReleased()
    {
        if (!canCutJump)
            return;

        if (!jumpHeld && velocity.y > 0f)
        {
            velocity.y *= jumpCutMultiplier;
            canCutJump = false;
        }
        else if (velocity.y <= 0f)
        {
            canCutJump = false;
        }
    }

    void TryJump()
    {
        if (Time.time - lastJumpPressedTime > jumpBufferTime)
            return;

        if (Time.time - lastGroundedTime <= coyoteTime || State == PlayerState.Grappled)
        {
            Jump(jumpHeight);
            return;
        }

        int wall = WallSide(WallJumpDistance);
        if (wall != 0)
        {
            WallJump(wall);
        }
        else if (airJumpsLeft > 0)
        {
            airJumpsLeft--;
            Jump(airJumpHeight);
        }
    }

    void Jump(float height)
    {
        velocity.y = JumpVelocity(height);
        isGrounded = false;
        isWallSliding = false;
        canCutJump = true;
        lastJumpTime = Time.time;
        lastJumpPressedTime = float.NegativeInfinity;
        lastGroundedTime = float.NegativeInfinity;
    }

    void WallJump(int wallSide)
    {
        velocity.x = -wallSide * wallJumpPushSpeed;
        wallJumpTime = Time.time;
        airJumpsLeft = airJumps;
        Jump(jumpHeight);
    }

    void ApplyGravity()
    {
        if (isGrounded)
        {
            velocity -= groundNormal * GroundStickSpeed;
            return;
        }

        float gravity = Gravity;
        if (velocity.y < 0f)
            gravity *= fallGravityMultiplier;
        if (jumpHeld && Mathf.Abs(velocity.y) < ApexSpeed)
            gravity *= ApexGravityMultiplier;

        float maxFall = isWallSliding ? wallSlideSpeed : maxFallSpeed;
        velocity.y = Mathf.MoveTowards(velocity.y, -maxFall, gravity * Time.fixedDeltaTime);
    }

    float JumpVelocity(float height)
    {
        float g = Gravity;
        float gdt = g * Time.fixedDeltaTime;
        return 0.5f * (gdt + Mathf.Sqrt(gdt * gdt + 8f * g * height));
    }

    int WallSide(float distance)
    {
        if (IsWall(Vector2.right, distance))
            return 1;
        if (IsWall(Vector2.left, distance))
            return -1;
        return 0;
    }

    bool IsWall(Vector2 direction, float distance)
    {
        RaycastHit2D hit = Cast(direction, distance, WallCheckHeight);
        return hit && Mathf.Abs(hit.normal.x) > WallNormalThreshold;
    }

    RaycastHit2D Cast(Vector2 direction, float distance, float heightScale, bool includeOneWay = false)
    {
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x - 2f * Skin, b.size.y * heightScale - 2f * Skin);
        return CastFrom(b.center, size, direction, distance + Skin, includeOneWay);
    }

    RaycastHit2D CastFrom(Vector2 center, Vector2 size, Vector2 direction, float distance, bool includeOneWay = false)
    {
        Physics2D.BoxCast(center, size, 0f, direction, solidFilter, castHits, distance);
        RaycastHit2D nearest = default;
        foreach (RaycastHit2D hit in castHits)
        {
            if (BlocksCast(hit, includeOneWay) && (!nearest || hit.distance < nearest.distance))
                nearest = hit;
        }
        return nearest;
    }

    bool BlocksCast(RaycastHit2D hit, bool includeOneWay)
    {
        if (!IsGroundCollider(hit.collider))
            return false;

        if (!IsOneWay(hit.collider))
            return true;

        return includeOneWay && hit.distance > 0f && !droppedPlatforms.Contains(hit.collider);
    }

    bool Overlaps(Vector2 center, Vector2 size)
    {
        Physics2D.OverlapBox(center, size, 0f, solidFilter, overlapHits);
        foreach (Collider2D hit in overlapHits)
        {
            if (IsGroundCollider(hit) && !IsOneWay(hit))
                return true;
        }
        return false;
    }

    bool IsGroundCollider(Collider2D candidate)
    {
        if (candidate.attachedRigidbody == rb || candidate.transform.IsChildOf(transform))
            return false;

        return (groundLayer.value & (1 << candidate.gameObject.layer)) != 0 ||
            IsOneWay(candidate) ||
            candidate.GetComponentInParent<MovingPlatform>() != null ||
            candidate.GetComponentInParent<VerticalPlatform>() != null;
    }

    static bool IsOneWay(Collider2D candidate) =>
        candidate.usedByEffector && candidate.TryGetComponent(out PlatformEffector2D effector) && effector.useOneWay;

    static Vector2 PlatformVelocity(Collider2D platform)
    {
        VerticalPlatform vertical = platform.GetComponentInParent<VerticalPlatform>();
        if (vertical != null && vertical.isActiveAndEnabled)
            return vertical.Velocity;

        MovingPlatform moving = platform.GetComponentInParent<MovingPlatform>();
        if (moving != null && moving.isActiveAndEnabled)
            return moving.Velocity;

        return Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        float left = b.min.x - 0.5f;
        float right = b.max.x + 0.5f;

        Gizmos.color = Color.cyan;
        float jumpTop = b.min.y + jumpHeight;
        Gizmos.DrawLine(new Vector3(left, jumpTop), new Vector3(right, jumpTop));

        if (airJumps > 0)
        {
            Gizmos.color = Color.blue;
            float airJumpTop = jumpTop + airJumpHeight;
            Gizmos.DrawLine(new Vector3(left, airJumpTop), new Vector3(right, airJumpTop));
        }
    }
}