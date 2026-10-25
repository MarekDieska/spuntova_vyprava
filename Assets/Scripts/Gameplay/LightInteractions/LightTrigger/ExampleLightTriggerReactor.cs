using UnityEngine;

public class ExampleLightTriggerReactor : MonoBehaviour
{
    public void ToggleSomething(bool isActivated)
    {
        Debug.Log($"YEAHOOOO! {isActivated}");
        gameObject.SetActive(!isActivated);
    }
}
