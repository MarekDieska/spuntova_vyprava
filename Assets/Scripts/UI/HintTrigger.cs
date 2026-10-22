using System.Collections;
using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    public GameObject popup;

    CanvasGroup group;
    bool was_shown = false;

    void Awake()
    {
        group = popup.GetComponent<CanvasGroup>();

        group.alpha = 0;
        popup.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (was_shown == true)
        {
            return;
        }

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
        {
            return;
        }

        was_shown = true;
        StartCoroutine(ShowPopup());
    }

    IEnumerator ShowPopup()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        group.alpha = 0;
        popup.SetActive(true);

        yield return null;

        float time = 0;

        while (time < 1)
        {
            time = time + Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            group.alpha = Mathf.Clamp01(time);

            yield return null;
        }

        yield return new WaitForSecondsRealtime(3);

        while (group.alpha > 0)
        {
            group.alpha = group.alpha - Time.unscaledDeltaTime * 3;

            yield return null;
        }

        group.alpha = 0;
        popup.SetActive(false);
    }
}