using System.Collections.Generic;
using UnityEngine;

public class LightDetector : MonoBehaviour
{
    private HashSet<ILightReactable> activeRelatables = new HashSet<ILightReactable>();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out ILightReactable reactable))
        {
            activeRelatables.Add(reactable);
            reactable.OnLightEnter();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out ILightReactable reactable))
        {
            activeRelatables.Remove(reactable);
            reactable.OnLightExit();
        }
    }
    private void Update()
    {
        activeRelatables.RemoveWhere(r => r == null);
        foreach (var reactable in activeRelatables)
        {
            reactable.OnLightStay(Time.deltaTime);
        }
    }
}
