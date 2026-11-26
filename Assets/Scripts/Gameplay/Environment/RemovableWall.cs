using UnityEngine;

// Put on a RemovableWall object with a SpriteRenderer and a Collider2D
// that a flower toggles via GameObject.SetActive.
// If the wall turns back on while the player is inside it, it either stays
// non-solid (ghost) until the player leaves, or damages and respawns the player.
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class RemovableWall : MonoBehaviour
{
    private enum PlayerInsideMode { WaitUntilClear, DamageAndRespawn }

    [SerializeField] private PlayerInsideMode whenPlayerInside = PlayerInsideMode.WaitUntilClear;
    [SerializeField] private int damageAmount = 1;
    [SerializeField, Range(0f, 1f)] private float ghostAlpha = 0.4f;

    private SpriteRenderer spriteRenderer;
    private Collider2D wallCollider;
    private Color originalColor;
    private PlayerHealth player;
    private BoxCollider2D playerCollider;
    private bool waitingForPlayer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        wallCollider = GetComponent<Collider2D>();
        originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        if (!PlayerInside()) return;

        if (whenPlayerInside == PlayerInsideMode.DamageAndRespawn)
        {
            player.TakeDamage(damageAmount);
            player.Respawn();
            return;
        }

        SetSolid(false);
    }

    private void Update()
    {
        if (waitingForPlayer && !PlayerInside())
            SetSolid(true);
    }

    private void OnDisable()
    {
        if (waitingForPlayer)
            SetSolid(true);
    }

    private void SetSolid(bool solid)
    {
        waitingForPlayer = !solid;
        wallCollider.enabled = solid;

        Color color = originalColor;
        if (!solid) color.a *= ghostAlpha;
        spriteRenderer.color = color;
    }

    private bool PlayerInside()
    {
        if (playerCollider == null)
        {
            player = FindAnyObjectByType<PlayerHealth>();
            if (player == null) return false;
            playerCollider = player.GetComponent<BoxCollider2D>();
            if (playerCollider == null) return false;
        }

        // Shrink a bit so just touching the wall's edge doesn't count as inside.
        Bounds playerBounds = playerCollider.bounds;
        playerBounds.Expand(-0.05f);

        // Disabled colliders report zero-size bounds in Unity,
        // so briefly enable to read accurate world bounds.
        bool wasEnabled = wallCollider.enabled;
        if (!wasEnabled) wallCollider.enabled = true;

        bool overlaps = wallCollider.bounds.Intersects(playerBounds);

        if (!wasEnabled) wallCollider.enabled = false;

        return overlaps;
    }
}