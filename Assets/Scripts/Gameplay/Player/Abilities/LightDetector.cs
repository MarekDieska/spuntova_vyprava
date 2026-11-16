using System.Collections.Generic;
using UnityEngine;

public class LightDetector : MonoBehaviour
{
    private HashSet<ILightReactable> activeRelatables = new HashSet<ILightReactable>();
    private readonly List<ILightReactable> iterationBuffer = new List<ILightReactable>();
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
        activeRelatables.RemoveWhere(r => r == null || (r is Object unityObj && unityObj == null));

        iterationBuffer.Clear();
        iterationBuffer.AddRange(activeRelatables);

        for (int i = 0; i < iterationBuffer.Count; i++)
        {
            ILightReactable reactable = iterationBuffer[i];
            if (activeRelatables.Contains(reactable))
            {
                reactable.OnLightStay(Time.deltaTime);
            }
        }
    }
}
