public interface ILightReactable
{
    void OnLightEnter();
    void OnLightStay(float deltaTime);
    void OnLightExit();
}
