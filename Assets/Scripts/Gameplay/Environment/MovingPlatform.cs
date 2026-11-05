
using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class MovingPlatform : MonoBehaviour
{
    public float speed = 4f;

    public Vector2 Velocity { get; private set; }

    private Rigidbody2D body;
    private float startX;
    private float leftX;
    private int direction = -1;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (body == null)
            body = gameObject.AddComponent<Rigidbody2D>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.freezeRotation = true;
    }

    void Start()
    {
        startX = body.position.x;
        leftX = startX;

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("MovingPlatform requires a Collider2D.", this);
            enabled = false;
            return;
        }

        RaycastHit2D[] hits = Physics2D.BoxCastAll(col.bounds.center, col.bounds.size,
            0f, Vector2.left, Mathf.Infinity, LayerMask.GetMask("Ground"));
        float nearestWall = Mathf.Infinity;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.isTrigger || hit.collider.attachedRigidbody == body ||
                hit.collider.transform.IsChildOf(transform) || hit.normal.x < 0.5f)
                continue;

            nearestWall = Mathf.Min(nearestWall, hit.distance);
        }

        if (!float.IsPositiveInfinity(nearestWall))
            leftX = startX - nearestWall;
    }

    void FixedUpdate()
    {
        float targetX = direction < 0 ? leftX : startX;

        Vector2 position = body.position;
        position.x = Mathf.MoveTowards(position.x, targetX, Mathf.Max(0f, speed) * Time.fixedDeltaTime);
        Velocity = (position - body.position) / Time.fixedDeltaTime;
        body.MovePosition(position);

        if (position.x == targetX)
        {
            direction = -direction;
        }
    }

    void OnDisable()
    {
        Velocity = Vector2.zero;
        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
}
