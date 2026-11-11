using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGrass : MonoBehaviour
{
    [Header("Ground Grass")]
    [SerializeField] Tilemap ground;
    [SerializeField] Collider2D groundCollider;
    [SerializeField] GameObject[] grassPrefabs;
    [Range(0f, 1f)][SerializeField] float chance = 1f;
    [SerializeField, Min(0f)] float spacing = 0f;
    [SerializeField] float yOffset = 0f;
    [SerializeField, Min(0.1f)] float raycastHeight = 2f;
    [SerializeField, Range(0f, 10f)] float flatAngleTolerance = 2f;
    [SerializeField, Min(0f)] float edgeMargin = 0.02f;
    [SerializeField] string sortingLayer = "Player";
    [SerializeField] int sortingOrder = 5;
    [SerializeField] int seed = 12345;

    [Header("Special Close Layer")]
    [SerializeField] bool enableCloseLayer = false;
    [SerializeField] Camera gameCamera;
    [SerializeField] Transform player;
    [SerializeField] Sprite[] closeSprites = new Sprite[3];
    [SerializeField, Range(0f, 1f)] float closeFollowX = 0.2f;
    [SerializeField, Range(0f, 1f)] float closeFollowY = 0f;
    [SerializeField] float closeY = 0f;
    [SerializeField, Min(0.01f)] float closeScale = 1f;
    [SerializeField] string closeSortingLayer = "Player";
    [SerializeField] int closeSortingOrder = 50;
    [SerializeField, Min(0f)] float transparencyDistance = 2f;
    [SerializeField, Range(0f, 1f)] float minimumAlpha = 0.25f;
    [SerializeField] int closeSeed = 9876;
    [Header("Close Flower Rarity")]
    [SerializeField, Min(1f)] float closeMinGapScreens = 1f;
    [SerializeField, Min(1f)] float closeMaxGapScreens = 2f;

    struct SurfaceSample
    {
        public float x;
        public float y;
        public bool flat;
    }

    struct Placement
    {
        public GameObject prefab;
        public float width;
    }

    class CloseTile
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public Sprite sprite;
        public int index;
    }

    readonly List<CloseTile> closeTiles = new List<CloseTile>();
    readonly List<Sprite> validCloseSprites = new List<Sprite>();
    Transform closeContainer;
    float initialCameraX;
    float initialCameraY;
    float closeStartX;
    float closeTotalWidth;
    int closeFirstIndex;
    int closeLastIndex;
    bool closeReady;

    void Start()
    {
        if (Application.isPlaying && enableCloseLayer)
            InitializeCloseLayer();
    }

    void LateUpdate()
    {
        if (!Application.isPlaying || !enableCloseLayer) return;
        if (!closeReady) InitializeCloseLayer();
        if (closeReady) UpdateCloseLayer();
    }

    [ContextMenu("Generate Grass")]
    void GenerateGrass()
    {
        if (ground == null || grassPrefabs == null || grassPrefabs.Length == 0)
        {
            Debug.LogWarning("Assign Ground and Grass Prefabs.", this);
            return;
        }

        if (groundCollider == null)
            groundCollider = ground.GetComponent<Collider2D>();

        if (groundCollider == null)
        {
            Debug.LogWarning("Assign Ground Collider (TilemapCollider2D or CompositeCollider2D).", this);
            return;
        }

        var validPrefabs = new List<GameObject>();
        foreach (GameObject prefab in grassPrefabs)
        {
            if (prefab == null) continue;
            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null && GetWidth(prefab) > 0f)
                validPrefabs.Add(prefab);
        }
        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("Grass Prefabs must have a SpriteRenderer with a sprite.", this);
            return;
        }

        Transform previous = transform.Find("Generated Grass");
        if (previous != null) DestroyImmediate(previous.gameObject);
        GameObject container = new GameObject("Generated Grass");
        container.transform.SetParent(transform, false);

        Bounds bounds = groundCollider.bounds;
        float cellWidth = Mathf.Abs(ground.cellSize.x * ground.transform.lossyScale.x);
        float step = Mathf.Clamp(cellWidth / 20f, 0.01f, 0.08f);
        float heightTolerance = Mathf.Max(0.005f, step * 0.15f);
        var samples = new List<SurfaceSample>();

        for (float x = bounds.min.x + step * 0.5f; x < bounds.max.x; x += step)
        {
            SurfaceSample sample = new SurfaceSample { x = x, flat = false };
            Vector2 origin = new Vector2(x, bounds.max.y + raycastHeight);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down,
                bounds.size.y + raycastHeight * 2f);
            float highestY = float.NegativeInfinity;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != groundCollider || hit.normal.y <= 0f) continue;
                if (hit.point.y <= highestY) continue;
                highestY = hit.point.y;
                sample.y = hit.point.y;
                sample.flat = Vector2.Angle(hit.normal, Vector2.up) <= flatAngleTolerance;
            }
            samples.Add(sample);
        }

        var random = new System.Random(seed);
        int start = 0;
        while (start < samples.Count)
        {
            if (!samples[start].flat) { start++; continue; }
            float y = samples[start].y;
            int end = start;
            while (end + 1 < samples.Count && samples[end + 1].flat &&
                   Mathf.Abs(samples[end + 1].y - y) <= heightTolerance)
                end++;

            float left = samples[start].x + edgeMargin;
            float right = samples[end].x - edgeMargin;
            float available = right - left;

            if (available > 0f)
            {
                var row = new List<Placement>();
                float used = 0f;
                while (true)
                {
                    int first = random.Next(validPrefabs.Count);
                    GameObject chosen = null;
                    float chosenWidth = 0f;
                    for (int i = 0; i < validPrefabs.Count; i++)
                    {
                        GameObject prefab = validPrefabs[(first + i) % validPrefabs.Count];
                        float width = GetWidth(prefab);
                        float required = width + (row.Count > 0 ? spacing : 0f);
                        if (used + required <= available + 0.0001f)
                        {
                            chosen = prefab;
                            chosenWidth = width;
                            break;
                        }
                    }
                    if (chosen == null) break;
                    used += chosenWidth + (row.Count > 0 ? spacing : 0f);
                    row.Add(new Placement { prefab = chosen, width = chosenWidth });
                }

                float cursor = left + (available - used) * 0.5f;
                foreach (Placement placement in row)
                {
                    float centerX = cursor + placement.width * 0.5f;
                    cursor += placement.width + spacing;
                    if (random.NextDouble() > chance) continue;

                    SpriteRenderer source = placement.prefab.GetComponent<SpriteRenderer>();
                    Vector3 scale = placement.prefab.transform.localScale;
                    float pivotX = (source.sprite.bounds.min.x + source.sprite.bounds.max.x) * 0.5f * scale.x;
                    float bottomY = source.sprite.bounds.min.y * scale.y;
                    if (scale.y < 0f) bottomY = source.sprite.bounds.max.y * scale.y;
                    Vector3 position = new Vector3(centerX - pivotX, y - bottomY + yOffset, 0f);

                    GameObject instance = Instantiate(placement.prefab, position,
                        placement.prefab.transform.rotation, container.transform);
                    instance.name = placement.prefab.name;
                    SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
                    renderer.sortingLayerName = sortingLayer;
                    renderer.sortingOrder = sortingOrder;
                }
            }
            start = end + 1;
        }
    }

    static float GetWidth(GameObject prefab)
    {
        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        return sr.sprite.bounds.size.x * Mathf.Abs(prefab.transform.localScale.x);
    }

    void InitializeCloseLayer()
    {
        closeReady = false;
        if (gameCamera == null) gameCamera = Camera.main;
        if (gameCamera == null || !gameCamera.orthographic) return;

        validCloseSprites.Clear();
        foreach (Sprite sprite in closeSprites)
            if (sprite != null && sprite.bounds.size.x > 0f) validCloseSprites.Add(sprite);
        if (validCloseSprites.Count == 0) return;

        if (closeContainer != null) Destroy(closeContainer.gameObject);
        closeTiles.Clear();
        closeContainer = new GameObject("Special Close Layer").transform;
        closeContainer.SetParent(transform, false);
        initialCameraX = gameCamera.transform.position.x;
        initialCameraY = gameCamera.transform.position.y;
        closeStartX = initialCameraX;
        closeFirstIndex = 0;
        closeLastIndex = -1;
        closeTotalWidth = 0f;
        closeReady = true;
        UpdateCloseLayer();
    }

    void UpdateCloseLayer()
    {
        float cameraX = gameCamera.transform.position.x;
        float cameraY = gameCamera.transform.position.y;
        float anchorX = closeStartX + (cameraX - initialCameraX) * closeFollowX;
        float anchorY = closeY + (cameraY - initialCameraY) * closeFollowY;
        float halfWidth = gameCamera.orthographicSize * gameCamera.aspect;
        float visibleLeft = cameraX - halfWidth;
        float visibleRight = cameraX + halfWidth;
        float buffer = Mathf.Max(2f, halfWidth * 0.5f);

        // One full camera width (or more) of empty space between flower edges.
        float screenWidth = 2f * halfWidth;
        float minGap = Mathf.Max(1f, closeMinGapScreens) * screenWidth;
        float maxGap = Mathf.Max(minGap, closeMaxGapScreens * screenWidth);
        float minSpriteWidth = float.PositiveInfinity;
        foreach (Sprite sprite in validCloseSprites)
            minSpriteWidth = Mathf.Min(minSpriteWidth, sprite.bounds.size.x * closeScale);
        int count = Mathf.Clamp(Mathf.CeilToInt(
            (2f * halfWidth + 2f * buffer) / Mathf.Max(0.01f, minSpriteWidth + minGap)) + 5, 3, 300);

        int startIndex = 0;
        float startLeft = anchorX;
        while (startLeft > visibleLeft - buffer && startIndex > -10000)
        {
            startIndex--;
            startLeft -= TileWidth(startIndex) + GapAfter(startIndex, minGap, maxGap);
        }
        while (startLeft + TileWidth(startIndex) < visibleLeft - buffer && startIndex < 10000)
        {
            startLeft += TileWidth(startIndex) + GapAfter(startIndex, minGap, maxGap);
            startIndex++;
        }

        float cursor = startLeft;
        int index = startIndex;
        int needed = 0;
        while (cursor < visibleRight + buffer && needed < count)
        {
            Sprite sprite = SpriteForIndex(index);
            if (needed >= closeTiles.Count) CreateCloseTile();
            CloseTile tile = closeTiles[needed];
            tile.sprite = sprite;
            tile.index = index;
            tile.renderer.sprite = sprite;
            tile.renderer.sortingLayerName = closeSortingLayer;
            tile.renderer.sortingOrder = closeSortingOrder;
            tile.transform.localScale = new Vector3(closeScale, closeScale, 1f);
            // Sprite bounds min compensates for arbitrary pivot location.
            float leftOffset = sprite.bounds.min.x * closeScale;
            tile.transform.position = new Vector3(cursor - leftOffset, anchorY, 0f);
            tile.transform.gameObject.SetActive(true);
            UpdateTileAlpha(tile);
            cursor += sprite.bounds.size.x * closeScale + GapAfter(index, minGap, maxGap);
            index++;
            needed++;
        }
        for (int i = needed; i < closeTiles.Count; i++)
            closeTiles[i].transform.gameObject.SetActive(false);
    }

    float TileWidth(int index)
    {
        return SpriteForIndex(index).bounds.size.x * closeScale;
    }

    float GapAfter(int index, float minGap, float maxGap)
    {
        unchecked
        {
            uint hash = (uint)index * 2246822519u + (uint)closeSeed * 3266489917u;
            hash ^= hash >> 15;
            hash *= 2246822519u;
            hash ^= hash >> 13;
            float t = (hash & 0xFFFFFFu) / 16777215f;
            return Mathf.Lerp(minGap, maxGap, t);
        }
    }

    Sprite SpriteForIndex(int index)
    {
        unchecked
        {
            uint x = (uint)(index * 374761393 + closeSeed * 668265263);
            x = (x ^ (x >> 13)) * 1274126177u;
            x ^= x >> 16;
            return validCloseSprites[(int)(x % (uint)validCloseSprites.Count)];
        }
    }

    void CreateCloseTile()
    {
        GameObject obj = new GameObject("Close Tile");
        obj.transform.SetParent(closeContainer, false);
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        closeTiles.Add(new CloseTile { transform = obj.transform, renderer = renderer });
    }

    void UpdateTileAlpha(CloseTile tile)
    {
        float alpha = 1f;
        if (player != null && transparencyDistance > 0f)
        {
            // Distance to the sprite rectangle, not merely to its pivot.
            Bounds bounds = tile.renderer.bounds;
            float nearestX = Mathf.Clamp(player.position.x, bounds.min.x, bounds.max.x);
            float nearestY = Mathf.Clamp(player.position.y, bounds.min.y, bounds.max.y);
            float distance = Vector2.Distance(new Vector2(player.position.x, player.position.y),
                new Vector2(nearestX, nearestY));
            alpha = Mathf.Lerp(minimumAlpha, 1f,
                Mathf.Clamp01(distance / transparencyDistance));
        }
        Color color = tile.renderer.color;
        color.a = alpha;
        tile.renderer.color = color;
    }
}
