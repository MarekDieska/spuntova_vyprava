using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public event Action<int, int, int> OnHealthChangedAction; //<current, max, deltaChange>
    public event Action OnDeathAction;
    [SerializeField] private int maxHp = 3;

    private int currentHp;
    private bool isInvincible = false;
    private Vector2 currentRespawnPosition;
    private CameraController2D cameraController;

    private void Awake()
    {
        currentHp = maxHp;
        if (Camera.main != null)
        {
            cameraController = Camera.main.GetComponent<CameraController2D>();
        }
    }
    private void Start()
    {
        OnHealthChangedAction?.Invoke(currentHp, maxHp, 0);
        currentRespawnPosition = transform.position;
    }
    public void TakeDamage(int amount)
    {
        if (isInvincible || currentHp <= 0) return;

        currentHp = Mathf.Max(0, currentHp - amount);

        OnHealthChangedAction?.Invoke(currentHp, maxHp, amount);
        if (currentHp == 0)
        {
            OnDeathAction?.Invoke();
        }
    }
    public void SetInvincible(bool state)
    {
        isInvincible = state;
    }
    public void SetRespawnPoint(Vector2 newPosition)
    {
        currentRespawnPosition = newPosition;
    }
    public void Respawn()
    {
        transform.position = currentRespawnPosition;
        if (cameraController != null)
        {
            cameraController.RespawnTransition();
        }
    }

    [SerializeField] private int hpAdjust = 1;
    //SIMULATE DIFFUCULTY CHANGE IF WORKS
    [ContextMenu("Trigger Max Health Adjustment")]
    public void ChangeHealth()
    {
        maxHp = hpAdjust;
        currentHp = maxHp;
        OnHealthChangedAction?.Invoke(currentHp, maxHp, 0);
    }
}
