using UnityEngine;
using UnityEngine.Tilemaps;

// Put on a RemovableWall tilemap that a flower toggles via GameObject.SetActive.
// If the wall turns back on while the player is inside it, it either stays
// non-solid (ghost) until the player leaves, or damages and respawns the player.
[RequireComponent(typeof(Tilemap))]
public class RemovableWall : MonoBehaviour
{
    private enum PlayerInsideMode { WaitUntilClear, DamageAndRespawn }

    [SerializeField] private PlayerInsideMode whenPlayerInside = PlayerInsideMode.WaitUntilClear;
    [SerializeField] private int damageAmount = 1;
    [SerializeField, Range(0f, 1f)] private float ghostAlpha = 0.4f;

    private Tilemap tilemap;
    private Collider2D[] colliders;
    private Color originalColor;
    private PlayerHealth player;
    private BoxCollider2D playerCollider;
    private bool waitingForPlayer;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        colliders = GetComponents<Collider2D>();
        originalColor = tilemap.color;
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

        foreach (Collider2D col in colliders)
            col.enabled = solid;

        Color color = originalColor;
        if (!solid) color.a *= ghostAlpha;
        tilemap.color = color;
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
        Bounds bounds = playerCollider.bounds;
        bounds.Expand(-0.05f);

        Vector3Int min = tilemap.WorldToCell(bounds.min);
        Vector3Int max = tilemap.WorldToCell(bounds.max);
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                if (tilemap.HasTile(new Vector3Int(x, y, 0)))
                    return true;
            }
        }
        return false;
    }
}
