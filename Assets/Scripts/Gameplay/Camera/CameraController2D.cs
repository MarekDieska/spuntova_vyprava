using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] PlayerMovement target;
    [SerializeField] Vector2 offset = new Vector2(0f, 1f);

    [Header("Follow")]
    [SerializeField] float horizontalSmoothTime = 0.35f;
    [SerializeField] float verticalSmoothTime = 0.6f;
    [SerializeField] float lookAhead = 1.5f;
    [SerializeField] float lookAheadSmoothTime = 1.5f;
    [SerializeField] float jumpFollowHeight = 4f;
    [SerializeField, Range(0.2f, 1f)] float safeArea = 0.8f;

    [Header("Fall")]
    [SerializeField] float longFallDistance = 8f;
    [SerializeField] float fallLookAhead = 1.5f;

    [Header("Zoom")]
    [SerializeField, Min(0.1f)] float size = 8f;
    [SerializeField, Range(0f, 1f)] float runZoomOut = 0f;
    [SerializeField, Range(0f, 1f)] float fallZoomOut = 0.1f;
    [SerializeField] float zoomOutTime = 1.2f;
    [SerializeField] float zoomInTime = 2f;

    [Header("Bounds")]
    [SerializeField] BoxCollider2D levelBounds = null;

    const float MoveThreshold = 1f;
    const float TurnDelay = 0.4f;
    const float FallSmoothTime = 0.2f;
    const float FallBlendTime = 0.5f;
    const float CatchUpStart = 0.5f;
    const float CatchUpStrength = 0.6f;

    Camera cam;
    Rigidbody2D body;
    float anchorY;
    float groundY;
    float facing = 1f;
    float turnTimer;
    float lookAheadX;
    float lookAheadVelocity;
    float fallWeight;
    float sizeVelocity;
    Vector2 followVelocity;
    Vector3 transitionStart;
    float transitionTime;
    float transitionDuration;

    void Reset() => target = FindAnyObjectByType<PlayerMovement>();

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (levelBounds != null)
            levelBounds.isTrigger = true;
    }

    void Start()
    {
        if (target == null)
            target = FindAnyObjectByType<PlayerMovement>();
        if (target == null)
        {
            Debug.LogError("CameraController2D has no PlayerController2D to follow.", this);
            enabled = false;
            return;
        }
        body = target.GetComponent<Rigidbody2D>();
        Snap();
    }

    public void Snap()
    {
        if (target == null || cam == null)
            return;

        Vector2 player = target.transform.position;
        anchorY = groundY = player.y;
        lookAheadX = facing * lookAhead;
        lookAheadVelocity = sizeVelocity = fallWeight = turnTimer = 0f;
        followVelocity = Vector2.zero;
        transitionTime = transitionDuration = 0f;

        cam.orthographicSize = Mathf.Min(BaseSize(player), MaxSizeInBounds());
        transform.position = ClampToBounds(DesiredPosition(player));
    }

    public void ChangeSection(BoxCollider2D newBounds, float duration)
    {
        levelBounds = newBounds;
        transitionStart = transform.position;
        transitionTime = 0f;
        transitionDuration = duration;
        followVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (target == null || dt <= 0f)
            return;

        Vector2 player = target.transform.position;
        Vector2 velocity = body.linearVelocity;

        UpdateAnchor(player);
        UpdateLookAhead(velocity.x, dt);
        UpdateFall(player, velocity.y, dt);
        UpdateZoom(player, velocity.x);

        if (transitionTime < transitionDuration)
        {
            transitionTime += dt;
            float progress = Mathf.Clamp01(transitionTime / transitionDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            Vector3 destination = ClampToBounds(DesiredPosition(player));

            // During the transition, the camera can travel between both sections.
            transform.position = Vector3.Lerp(transitionStart, destination, smoothProgress);
            return;
        }

        Follow(player);
    }

    void UpdateAnchor(Vector2 player)
    {
        if (target.IsGrounded || target.IsWallSliding)
        {
            anchorY = player.y;
            groundY = player.y;
        }
        else if (player.y < anchorY)
        {
            anchorY = player.y;
        }
        else if (player.y > anchorY + jumpFollowHeight)
        {
            anchorY = player.y - jumpFollowHeight;
        }
    }

    void UpdateLookAhead(float velocityX, float dt)
    {
        float direction = Mathf.Abs(velocityX) > MoveThreshold ? Mathf.Sign(velocityX) : facing;
        turnTimer = direction != facing ? turnTimer + dt : 0f;
        if (turnTimer >= TurnDelay)
        {
            facing = direction;
            turnTimer = 0f;
        }

        float desired = target.IsWallSliding ? 0f : facing * lookAhead;
        lookAheadX = Mathf.SmoothDamp(lookAheadX, desired, ref lookAheadVelocity, lookAheadSmoothTime);
    }

    void UpdateFall(Vector2 player, float velocityY, float dt)
    {
        float fallen = velocityY < 0f ? groundY - player.y : 0f;
        float desired = Mathf.InverseLerp(longFallDistance * 0.3f, longFallDistance, fallen);
        fallWeight = Mathf.MoveTowards(fallWeight, desired, dt / FallBlendTime);
    }

    void UpdateZoom(Vector2 player, float velocityX)
    {
        float moving = Mathf.Abs(velocityX) > MoveThreshold ? runZoomOut : 0f;
        float zoomOut = Mathf.Max(moving, fallWeight * fallZoomOut);
        float desired = Mathf.Min(BaseSize(player) * (1f + zoomOut), MaxSizeInBounds());
        float smoothTime = desired > cam.orthographicSize ? zoomOutTime : zoomInTime;
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, desired, ref sizeVelocity, smoothTime);
    }

    void Follow(Vector2 player)
    {
        Vector3 desired = ClampToBounds(DesiredPosition(player));
        Vector3 position = transform.position;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float horizontalTime = horizontalSmoothTime * CatchUp(player.x - position.x, halfWidth);
        float verticalTime = Mathf.Lerp(verticalSmoothTime, FallSmoothTime, fallWeight) * CatchUp(player.y - position.y, halfHeight);
        position.x = Mathf.SmoothDamp(position.x, desired.x, ref followVelocity.x, horizontalTime);
        position.y = Mathf.SmoothDamp(position.y, desired.y, ref followVelocity.y, verticalTime);

        float maxX = halfWidth * safeArea;
        float maxY = halfHeight * safeArea;
        position.x = Mathf.Clamp(position.x, player.x - maxX, player.x + maxX);
        position.y = Mathf.Clamp(position.y, player.y - maxY, player.y + maxY);

        transform.position = ClampToBounds(position);
    }

    float CatchUp(float distance, float halfScreen)
    {
        float t = Mathf.InverseLerp(safeArea * CatchUpStart, safeArea, Mathf.Abs(distance) / halfScreen);
        return 1f - CatchUpStrength * t;
    }

    Vector3 DesiredPosition(Vector2 player)
    {
        float x = player.x + offset.x + lookAheadX;
        float y = anchorY + offset.y - fallWeight * fallLookAhead;
        return new Vector3(x, y, transform.position.z);
    }

    float BaseSize(Vector2 player)
    {
        return size;
    }

    float MaxSizeInBounds()
    {
        if (levelBounds == null)
            return float.PositiveInfinity;

        Bounds b = levelBounds.bounds;
        return Mathf.Min(b.extents.y, b.extents.x / cam.aspect);
    }

    Vector3 ClampToBounds(Vector3 position)
    {
        if (levelBounds == null)
            return position;

        Bounds b = levelBounds.bounds;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        position.x = ClampAxis(position.x, b.min.x + halfWidth, b.max.x - halfWidth);
        position.y = ClampAxis(position.y, b.min.y + halfHeight, b.max.y - halfHeight);
        return position;
    }

    static float ClampAxis(float value, float min, float max) =>
        min > max ? (min + max) * 0.5f : Mathf.Clamp(value, min, max);
}
