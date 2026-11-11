using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDamageVisuals : MonoBehaviour
{
    [Header("Invincibility Settings")]
    [SerializeField] private float iFrameDuration = 1.5f;
    [SerializeField] private float blinkInterval = 0.1f;

    private SpriteRenderer sr;
    private PlayerHealth health;
    private CameraController2D cameraController;
    private RawImage damageOverlay;
    private Texture2D cornerTexture;
    private float damageFlash;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        health = GetComponent<PlayerHealth>();
        cameraController = Camera.main != null
            ? Camera.main.GetComponent<CameraController2D>()
            : FindFirstObjectByType<CameraController2D>();
        GameObject canvasObject = new GameObject("Damage Overlay", typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        GameObject imageObject = new GameObject("Red Corners", typeof(RectTransform), typeof(RawImage));
        imageObject.transform.SetParent(canvasObject.transform, false);
        damageOverlay = imageObject.GetComponent<RawImage>();
        damageOverlay.raycastTarget = false;
        damageOverlay.rectTransform.anchorMin = Vector2.zero;
        damageOverlay.rectTransform.anchorMax = Vector2.one;
        damageOverlay.rectTransform.offsetMin = Vector2.zero;
        damageOverlay.rectTransform.offsetMax = Vector2.zero;
        cornerTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        cornerTexture.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float edgeX = Mathf.Abs(x / 127f * 2f - 1f);
                float edgeY = Mathf.Abs(y / 127f * 2f - 1f);
                cornerTexture.SetPixel(x, y, new Color(1f, 0f, 0f, Mathf.Pow(edgeX * edgeY, 1.4f)));
            }
        }
        cornerTexture.Apply();
        damageOverlay.texture = cornerTexture;
        damageOverlay.color = Color.clear;
    }

    private void OnEnable() => health.OnHealthChangedAction += HandleHealthChanged;
    private void OnDisable()
    {
        health.OnHealthChangedAction -= HandleHealthChanged;
        damageFlash = 0f;
        damageOverlay.color = Color.clear;
    }

    private void Update()
    {
        damageOverlay.color = new Color(1f, 1f, 1f, damageFlash * 0.95f);
        damageFlash = Mathf.MoveTowards(damageFlash, 0f, Time.deltaTime / 0.15f);
    }

    private void OnDestroy()
    {
        Destroy(cornerTexture);
    }

    private void HandleHealthChanged(int currentHp, int maxHp, int amount)
    {
        if (amount > 0)
        {
            damageFlash = 1f;
            damageOverlay.color = new Color(1f, 1f, 1f, 0.95f);
            if (cameraController != null)
            {
                cameraController.Shake();
            }
        }
        if (amount < 0)
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        health.SetInvincible(true);

        float elapsed = 0f;
        while (elapsed < iFrameDuration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        sr.enabled = true;
        health.SetInvincible(false);
    }
}
