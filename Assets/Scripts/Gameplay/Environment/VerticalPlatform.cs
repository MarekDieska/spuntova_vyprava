using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class VerticalPlatform : MonoBehaviour
{
    public float speed = 3f;
    public float distance = 5f;

    public Vector2 Velocity
    {
        get;
        private set;
    }

    private Rigidbody2D body;
    private float startY;
    private bool movingUp = true;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.freezeRotation = true;
        startY = body.position.y;
    }

    void FixedUpdate()
    {
        float targetY = movingUp ? startY + Mathf.Max(0f, distance) : startY;
        Vector2 position = body.position;
        position.y = Mathf.MoveTowards(position.y, targetY, Mathf.Max(0f, speed) * Time.fixedDeltaTime);
        Velocity = (position - body.position) / Time.fixedDeltaTime;
        body.MovePosition(position);

        if (position.y == targetY)
        {
            movingUp = !movingUp;
        }
    }

    void OnDisable()
    {
        Velocity = Vector2.zero;
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }
}
