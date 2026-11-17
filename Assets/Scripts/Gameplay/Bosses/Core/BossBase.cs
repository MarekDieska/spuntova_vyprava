using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BossBase : MonoBehaviour, IDamageable, ILightReactable
{
    public event Action<int, int> OnHealthChanged; // <currentHp, maxHp>
    public event Action<bool> OnAgitationChanged;
    public event Action OnBossDefeated;

    [Header("Base Boss Stats")]
    [SerializeField] protected int maxHp = 3;
    [SerializeField] protected float maxLightExposureTime = 2f;
    [SerializeField] protected float agitationDrainDuration = 4f;
    [SerializeField] protected float hitStunDuration = 2f;
    [SerializeField] protected AbilityType rewardAbilities = AbilityType.None;
    [SerializeField] private Collider2D bossHitbox;

    public Rigidbody2D Rb { get; private set; }
    public BossStateMachine StateMachine { get; private set; }
    public Transform Target => target;

    public bool IsAgitated { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsFacingRight { get; private set; } = true;
    public bool IsEncounterActive { get; private set; }
    public BossDormantState DormantState { get; protected set; }
    public BossHitState HitState { get; protected set; }
    public int CurrentHp => currentHp;
    public float AgitationProgress => Mathf.Clamp01(currentLightExposureTime / maxLightExposureTime);

    protected int currentHp;
    protected bool isVulnerable = false;
    protected Transform target;

    private float currentLightExposureTime = 0f;
    private float lastLitTime = -999f; private bool isHitInvulnerable = false;
    private Vector3 initialSpawnPos;

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        StateMachine = new BossStateMachine();
        currentHp = maxHp;
        initialSpawnPos = transform.position;
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
        OnHealthChanged?.Invoke(currentHp, maxHp);
    }
    protected virtual void Update()
    {
        if (IsDead) return;
        bool isCurrentlyLit = (Time.time - lastLitTime) <= Time.fixedDeltaTime * 1.5f;
        if (!isCurrentlyLit && currentLightExposureTime > 0f)
        {
            float drainSpeed = maxLightExposureTime / agitationDrainDuration;

            currentLightExposureTime = Mathf.Max(0f, currentLightExposureTime - drainSpeed * Time.deltaTime);
            if (currentLightExposureTime <= 0f)
            {
                UpdateAgitationState(false);
            }
        }
        StateMachine.Tick();
    }

    protected virtual void FixedUpdate()
    {
        if (IsDead) return;
        StateMachine.FixedTick();
    }
    public virtual void OnLightEnter()
    {
        if (!IsEncounterActive) return;
        lastLitTime = Time.time;
    }
    public virtual void OnLightStay(float deltaTime)
    {
        if (!IsEncounterActive) return;
        lastLitTime = Time.time;
        currentLightExposureTime = Mathf.Min(maxLightExposureTime, currentLightExposureTime + deltaTime);
        if (currentLightExposureTime >= maxLightExposureTime)
        {
            UpdateAgitationState(true);
        }
    }
    public virtual void OnLightExit() { }
    protected virtual void UpdateAgitationState(bool agitated)
    {
        if (IsAgitated == agitated) return;
        IsAgitated = agitated;
        OnAgitationChanged?.Invoke(IsAgitated);
    }
    public void SetVulnerable(bool state)
    {
        isVulnerable = state;
    }
    public void SetHitInvulnerable(bool state)
    {
        isHitInvulnerable = state;
    }
    public virtual void TakeDamage(int amount)
    {
        bool canTakeDamage = IsAgitated || isVulnerable;
        if (IsDead || isHitInvulnerable || !canTakeDamage) return;
        currentHp = Mathf.Max(0, currentHp - amount);
        OnHealthChanged?.Invoke(currentHp, maxHp);
        if (currentHp == 0)
        {
            Die();
        }
        else
        {
            OnDamaged();
        }
    }
    protected virtual void OnDamaged()
    {
        Debug.Log("taken damage, trigger VFX");
        if (HitState != null)
        {
            StateMachine.ChangeState(HitState);
        }
    }
    public virtual void OnHitStateEnter() { }
    public virtual void OnHitStateExit() { }
    protected virtual void Die()
    {
        IsDead = true;
        StateMachine.CurrentState?.Exit();
        Rb.linearVelocity = Vector2.zero;
        if (rewardAbilities != AbilityType.None && PlayerAbilityManager.Instance != null)
        {
            PlayerAbilityManager.Instance.Unlock(rewardAbilities);
        }
        bossHitbox.enabled = false;
        OnBossDefeated?.Invoke();
    }
    public void FaceTarget()
    {
        if (target == null) return;
        float dirX = target.position.x - transform.position.x;
        if (dirX > 0.05f && !IsFacingRight) Flip();
        else if (dirX < -0.05f && IsFacingRight) Flip();
    }

    public void Flip()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    public float DistanceToTarget()
    {
        if (target == null) return float.MaxValue;
        return Vector2.Distance(transform.position, target.position);
    }
    [ContextMenu("Start Encounter")]
    public virtual void StartEncounter()
    {
        IsEncounterActive = true;
    }
    public virtual void ResetBoss()
    {
        if (IsDead) return;

        transform.position = initialSpawnPos;
        Rb.linearVelocity = Vector2.zero;

        currentHp = maxHp;
        OnHealthChanged?.Invoke(currentHp, maxHp);

        currentLightExposureTime = 0f;
        isVulnerable = false;
        isHitInvulnerable = false;
        IsEncounterActive = false;
        UpdateAgitationState(false);

        if (!IsFacingRight) Flip();

        if (DormantState != null)
        {
            StateMachine.ChangeState(DormantState);
        }
    }
}