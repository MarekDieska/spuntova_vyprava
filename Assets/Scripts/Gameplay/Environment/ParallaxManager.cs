using System.Collections.Generic;
using UnityEngine;

public class ParallaxManager : MonoBehaviour
{


    //KOD ESTE MENIM PRERABAM UPRAVUJEM, NEODPORUCAM DONHO POZERAT
    [SerializeField] Camera gameCamera;

    [Header("Grafika pozadia (za hracom)")]
    [SerializeField] Sprite[] veryFar;
    [SerializeField] Sprite[] far;
    [SerializeField] Sprite[] vines;
    [SerializeField] Sprite[] medium;
    [SerializeField] Sprite[] close;

    [Header("Grafika pred hracom")]
    [SerializeField] Sprite veryClose;
    [SerializeField] float lowestLevelY = -4.757f;

    class MovingLayer
    {
        public Sprite[] sprites;
        public Transform[] objects;
        public SpriteRenderer[,] images;
        public float[] positions;
        public float spacing;
        public int copies;
        public float follow;
        public float followY;
        public float minScale;
        public float maxScale;
        public float bottomY;
    }

    List<MovingLayer> layers = new List<MovingLayer>();
    List<SpriteRenderer> foreground = new List<SpriteRenderer>();
    float previousCameraX;
    float startCameraY;
    float screenWidth;
    float foregroundImageHeight;
    float foregroundVisibleHeight;
    SpriteRenderer foregroundFill;
    Sprite fillSprite;

    void Start()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        if (gameCamera == null)
        {
            enabled = false;
            return;
        }

        previousCameraX = gameCamera.transform.position.x;
        startCameraY = gameCamera.transform.position.y;
        screenWidth = gameCamera.orthographicSize * 2f * gameCamera.aspect;
        foregroundVisibleHeight = gameCamera.orthographicSize * 2f * 0.2f;
        foregroundImageHeight = foregroundVisibleHeight * 1.4f * 0.936f / 0.46f;
        float bottom = startCameraY - 10f;
        float flowerBottom = lowestLevelY - 3f;

        CreateLayer("VeryFar", veryFar, 4, 0.95f, -30, 0.15f, true, bottom, followY: 0.998f);
        CreateLayer("BetweenTrees", far, 1.5f, 0.9f, -25, 0.85f, true, bottom + 1f,
            0.5f, 0.7f, 0.994f, 0.72f);
        CreateLayer("Far", far, 4, 0.85f, -20, 0.55f, true, bottom, followY: 0.99f);
        CreateLayer("Vines", vines, 7, 0.85f, -19, 0.8f, true, bottom);
        CreateLayer("Medium", medium, 1, 0.6f, -10, 0.3f, false, flowerBottom, 0.8f, 1.25f, 0f);
        CreateLayer("Close", close, 1, 0.35f, -5, 0.7f, false, flowerBottom, 0.9f, 1.5f, 0f);
        CreateLayer("ForegroundFlowers", close, 0.25f, -0.15f, 50, 0.6f, false,
            flowerBottom, 1.6f, 2.1f, 0f);
    }

    void CreateLayer(string name, Sprite[] sprites, float visibleCount,
        float follow, int order, float offset, bool repeatUp, float bottom,
        float minScale = 0.7f, float maxScale = 1f, float followY = 0.95f, float opacity = 1f)
    {
        if (sprites == null)
        {
            return;
        }

        List<Sprite> valid = new List<Sprite>();

        foreach (Sprite sprite in sprites)
        {
            if (sprite != null)
            {
                valid.Add(sprite);
            }
        }

        if (valid.Count == 0)
        {
            return;
        }

        int copies = 1;

        if (repeatUp)
        {
            float shortestHeight = float.MaxValue;

            foreach (Sprite sprite in valid)
            {
                float spriteHeight = sprite.bounds.size.y * minScale;
                shortestHeight = Mathf.Min(shortestHeight, spriteHeight);
            }

            float coverage = gameCamera.orthographicSize * 2f + 10f;
            float safeHeight = Mathf.Max(0.01f, shortestHeight);
            copies = Mathf.CeilToInt(coverage / safeHeight) + 3;

            if (copies < 5)
            {
                copies = 5;
            }
        }

        GameObject parent = new GameObject(name);
        parent.transform.SetParent(transform, false);
        MovingLayer layer = new MovingLayer();
        layer.sprites = valid.ToArray();
        layer.spacing = screenWidth / visibleCount;
        layer.copies = copies;
        layer.follow = follow;
        layer.followY = followY;
        layer.minScale = minScale;
        layer.maxScale = maxScale;
        layer.bottomY = bottom;
        int count = Mathf.CeilToInt(visibleCount) + 4;
        layer.objects = new Transform[count];
        layer.images = new SpriteRenderer[count, copies];
        layer.positions = new float[count];

        for (int i = 0; i < count; i++)
        {
            GameObject obj = new GameObject("Decoration");
            obj.transform.SetParent(parent.transform, false);
            layer.objects[i] = obj.transform;
            layer.positions[i] = i - 2 + offset + Random.Range(-0.35f, 0.35f);

            for (int copy = 0; copy < copies; copy++)
            {
                SpriteRenderer renderer = CreateRenderer("Copy " + copy, obj.transform, order);
                renderer.color = new Color(1f, 1f, 1f, opacity);
                layer.images[i, copy] = renderer;
            }

            ChangeSprite(layer, i);
        }

        layers.Add(layer);
    }

    void LateUpdate()
    {
        float cameraX = gameCamera.transform.position.x;
        float movement = cameraX - previousCameraX;
        float verticalMovement = gameCamera.transform.position.y - startCameraY;

        foreach (MovingLayer layer in layers)
        {
            float spacing = layer.spacing;
            int count = layer.objects.Length;

            for (int i = 0; i < count; i++)
            {
                layer.positions[i] -= movement * (1f - layer.follow) / spacing;

                if (layer.positions[i] < -2f || layer.positions[i] >= count - 2f)
                {
                    while (layer.positions[i] < -2f)
                    {
                        layer.positions[i] += count;
                    }

                    while (layer.positions[i] >= count - 2f)
                    {
                        layer.positions[i] -= count;
                    }

                    ChangeSprite(layer, i);
                }

                float x = cameraX - screenWidth / 2f + layer.positions[i] * spacing;
                float y = layer.bottomY + verticalMovement * layer.followY;
                layer.objects[i].position = new Vector3(x, y, 0f);
            }
        }

        UpdateForeground(cameraX);
        previousCameraX = cameraX;
    }

    void ChangeSprite(MovingLayer layer, int index)
    {
        int spriteIndex = Random.Range(0, layer.sprites.Length);
        Sprite sprite = layer.sprites[spriteIndex];
        bool flipX = Random.value > 0.5f;
        float height = sprite.bounds.size.y;
        float scale = Random.Range(layer.minScale, layer.maxScale);
        layer.objects[index].localScale = new Vector3(scale, scale, 1f);
        float verticalOffset;

        if (layer.copies > 1)
        {
            verticalOffset = Random.Range(-height * 0.4f, 0f);
        }
        else
        {
            verticalOffset = Random.Range(-height * 0.1f, 0f);
        }

        for (int copy = 0; copy < layer.copies; copy++)
        {
            SpriteRenderer renderer = layer.images[index, copy];
            renderer.sprite = sprite;
            renderer.flipX = flipX;
            int tile = 0;

            if (layer.copies > 1)
            {
                tile = copy - 1;
            }

            renderer.flipY = tile % 2 != 0;
            Vector3 center = sprite.bounds.center;

            if (renderer.flipX)
            {
                center.x = -center.x;
            }

            if (renderer.flipY)
            {
                center.y = -center.y;
            }

            float copyY = height * (tile + 0.5f) + verticalOffset;
            Vector3 copyPosition = new Vector3(0f, copyY, 0f);
            renderer.transform.localPosition = copyPosition - center;
        }
    }

    void UpdateForeground(float cameraX)
    {
        if (veryClose == null)
        {
            return;
        }

        float lowering = foregroundImageHeight * 0.46f / 0.936f * 0.1f;
        float bottom = lowestLevelY - foregroundVisibleHeight - lowering;

        float scale = Mathf.Max(0.01f, foregroundImageHeight) / veryClose.bounds.size.y;
        float width = veryClose.bounds.size.x * scale;
        if (width <= 0f)
        {
            return;
        }

        float halfView = gameCamera.orthographicSize * gameCamera.aspect;
        int needed = Mathf.CeilToInt(halfView * 2f / width) + 4;

        while (foreground.Count < needed)
        {
            SpriteRenderer strip = CreateRenderer("VeryClose strip", transform, 100);
            foreground.Add(strip);
        }

        int first = Mathf.FloorToInt((cameraX - halfView) / width) - 1;

        for (int i = 0; i < foreground.Count; i++)
        {
            SpriteRenderer renderer = foreground[i];
            renderer.enabled = i < needed;
            renderer.sprite = veryClose;
            renderer.color = Color.white;
            renderer.transform.localScale = new Vector3(scale, scale, 1f);

            float stripX = (first + i) * width;
            float stripY = bottom + foregroundImageHeight / 2f;
            Vector3 stripPosition = new Vector3(stripX, stripY, 0f);
            renderer.transform.position = stripPosition - veryClose.bounds.center * scale;
        }
        if (foregroundFill == null)
        {
            foregroundFill = CreateRenderer("VeryClose dark ground", transform, 99);
            Texture2D texture = Texture2D.whiteTexture;
            Rect textureArea = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            fillSprite = Sprite.Create(texture, textureArea, pivot, texture.width);
            foregroundFill.sprite = fillSprite;
        }

        float fillBottom = gameCamera.transform.position.y - gameCamera.orthographicSize - 1f;
        float fillTop = bottom + 0.02f;
        foregroundFill.enabled = fillTop > fillBottom;
        if (!foregroundFill.enabled)
        {
            return;
        }

        foregroundFill.color = new Color(2f / 255f, 22f / 255f, 33f / 255f, 1f);
        float fillCenterY = (fillBottom + fillTop) / 2f;
        float fillHeight = fillTop - fillBottom;
        foregroundFill.transform.position = new Vector3(cameraX, fillCenterY, 0f);
        foregroundFill.transform.localScale = new Vector3(halfView * 2f + 2f, fillHeight, 1f);
    }

    void OnDestroy()
    {
        if (fillSprite != null)
        {
            Destroy(fillSprite);
        }
    }

    static SpriteRenderer CreateRenderer(string name, Transform parent, int order)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        if (order > 0)
        {
            renderer.sortingLayerName = "Player";
        }
        renderer.sortingOrder = order;
        return renderer;
    }
}
