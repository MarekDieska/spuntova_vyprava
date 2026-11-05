using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(1000)]
[RequireComponent(typeof(SpriteRenderer))]
public class RainOverlay : MonoBehaviour
{
    public Camera targetCamera;
    [Range(0f, 1f)] public float intensity = 1f;

    public float margin = 1f;

    SpriteRenderer _sr;

    void OnEnable()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null || _sr == null || _sr.sprite == null) return;

        float height = cam.orthographic
            ? cam.orthographicSize * 2f
            : 2f * Mathf.Abs(transform.position.z - cam.transform.position.z) * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * cam.aspect;

        Vector3 camPos = cam.transform.position;
        transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);
        transform.rotation = Quaternion.identity;

        Vector2 spriteSize = _sr.sprite.bounds.size;
        transform.localScale = new Vector3(
            (width + margin * 2f) / spriteSize.x,
            (height + margin * 2f) / spriteSize.y,
            1f);

        Color c = _sr.color;
        if (!Mathf.Approximately(c.a, intensity))
        {
            c.a = intensity;
            _sr.color = c;
        }
    }
}