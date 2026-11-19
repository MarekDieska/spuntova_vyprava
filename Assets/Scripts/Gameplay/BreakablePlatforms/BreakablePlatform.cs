using System.Collections;
using UnityEngine;

public class BreakablePlatform : MonoBehaviour
{
    [SerializeField] private float breakDelay = 1f;
    [SerializeField] private float minTopNormal = 0.5f;
    [SerializeField] private bool respawn = true;
    [SerializeField] private float respawnDelay = 3f;

    private bool isBreaking;
    private Collider2D[] colliders;
    private Renderer[] renderers;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider2D>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryBreak(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryBreak(collision);
    }

    private void TryBreak(Collision2D collision)
    {
        if (isBreaking || !collision.collider.TryGetComponent<PlayerMovement>(out _))
        {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y <= -minTopNormal)
            {
                isBreaking = true;
                StartCoroutine(Break());
                return;
            }
        }
    }

    private IEnumerator Break()
    {
        yield return new WaitForSeconds(breakDelay);

        if (!respawn)
        {
            gameObject.SetActive(false);
            yield break;
        }

        SetPlatformVisible(false);
        yield return new WaitForSeconds(respawnDelay);
        SetPlatformVisible(true);
        isBreaking = false;
    }

    private void SetPlatformVisible(bool visible)
    {
        foreach (Collider2D col in colliders)
        {
            col.enabled = visible;
        }

        foreach (Renderer rend in renderers)
        {
            rend.enabled = visible;
        }
    }
}
