using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
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

    public bool IsGrounded => isGrounded;
    public bool IsWallSliding => isWallSliding;
    public Vector2 Velocity => velocity;

    const float ContactDistance = 0.05f;
    const float SnapDistance = 0.3f;
    const float JumpGroundIgnoreTime = 0.1f;
    const float WallJumpDistance = 0.25f;
    const float Skin = 0.02f;
    const float GroundStickSpeed = 1f;
    const float ApexSpeed = 2f;
    const float ApexGravityMultiplier = 0.5f;
    const float CornerStep = 0.02f;
    const float StickDeadZone = 0.3f;

    Rigidbody2D rb;
    BoxCollider2D col;
    PlayerInput input;
    Vector2 velocity;

    float moveInput;
    bool jumpHeld;
    float lastJumpPressedTime = float.NegativeInfinity;

    float lastGroundedTime = float.NegativeInfinity;
    float wallJumpTime = float.NegativeInfinity;
    float lastJumpTime = float.NegativeInfinity;
    Vector2 groundNormal = Vector2.up;
    bool isGrounded;
    bool isWallSliding;
    bool canCutJump;
    int airJumpsLeft;

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
        // Read from PlayerInput every frame so we don't drop button presses
        moveInput = Mathf.Abs(input.MoveInput) < StickDeadZone ? 0f : Mathf.Sign(input.MoveInput);
        jumpHeld = input.JumpHeld;

        if (input.JumpPressedThisFrame)
        {
            lastJumpPressedTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        velocity = rb.linearVelocity;

        CheckSurroundings();
        Run();
        StepOverBumps();
        TryJump();
        CutJumpIfReleased();
        ApplyGravity();
        CorrectCorners();

        rb.linearVelocity = velocity;
    }

    void CheckSurroundings()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;
        groundNormal = Vector2.up;

        bool justJumped = Time.time - lastJumpTime < JumpGroundIgnoreTime;
        RaycastHit2D ground = Cast(Vector2.down, wasGrounded ? SnapDistance : ContactDistance, 1f);
        if (ground && !justJumped && ground.normal.y >= Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad))
        {
            isGrounded = true;
            groundNormal = ground.normal;
            lastGroundedTime = Time.time;
            airJumpsLeft = airJumps;

            float gap = ground.distance - Skin;
            if (gap > Skin)
                rb.position += Vector2.down * (gap - Skin * 0.5f);
        }

        int wall = isGrounded ? 0 : WallSide(ContactDistance);
        isWallSliding = wall != 0 && moveInput == wall && velocity.y <= 0f;
        if (isWallSliding)
            airJumpsLeft = airJumps;
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
        RaycastHit2D hit = Physics2D.BoxCast(feetCenter, feetSize, 0f, direction, reach, groundLayer);
        if (!hit || Mathf.Abs(hit.normal.x) < 0.9f)
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

    void TryJump()
    {
        if (Time.time - lastJumpPressedTime > jumpBufferTime)
            return;

        bool canGroundJump = Time.time - lastGroundedTime <= coyoteTime;
        int wall = WallSide(WallJumpDistance);

        if (canGroundJump)
        {
            Jump(jumpHeight);
        }
        else if (wall != 0)
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

    float JumpVelocity(float height)
    {
        float g = Gravity;
        float gdt = g * Time.fixedDeltaTime;
        return 0.5f * (gdt + Mathf.Sqrt(gdt * gdt + 8f * g * height));
    }

    int WallSide(float distance)
    {
        if (IsWall(Vector2.right, distance)) return 1;
        if (IsWall(Vector2.left, distance)) return -1;
        return 0;
    }

    bool IsWall(Vector2 direction, float distance)
    {
        RaycastHit2D hit = Cast(direction, distance, 0.6f);
        return hit && Mathf.Abs(hit.normal.x) > 0.9f;
    }

    RaycastHit2D Cast(Vector2 direction, float distance, float heightScale)
    {
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x - 2f * Skin, b.size.y * heightScale - 2f * Skin);
        return Physics2D.BoxCast(b.center, size, 0f, direction, distance + Skin, groundLayer);
    }

    bool Overlaps(Vector2 center, Vector2 size) => Physics2D.OverlapBox(center, size, 0f, groundLayer);

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