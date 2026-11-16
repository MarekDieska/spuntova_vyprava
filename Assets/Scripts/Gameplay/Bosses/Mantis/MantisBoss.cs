using UnityEngine;

public class MantisBoss : BossBase
{
    [Header("Default Variables")]
    [SerializeField] private float leapAttackChance = 0.65f;
    [SerializeField] private float swipeAttackChance = 0.65f;
    [SerializeField] private float decisionCooldown = 0.5f;
    [SerializeField] private float agitatedSpeedMultiplier = 1.6f;
    [Header("Swipe State")]
    [SerializeField] private float maxSwipeHeightDifference = 1.5f;
    [SerializeField] private float swipeTriggerRange = 4.5f;
    [SerializeField] private float swipeChargeTime = 0.5f;
    [SerializeField] private float timeBetweenSwipes = 0.125f;
    [SerializeField] private float swipeLungeSpeed = 4.5f;
    [SerializeField] private int swipeCount = 4;
    [SerializeField] private GameObject swipeHitbox;
    [Header("Leap State")]
    [SerializeField] private float leapChargeTime = 0.5f;
    [SerializeField] private float leapHorizontalForce = 12f;
    [SerializeField] private float leapVerticalForce = 14f;
    [SerializeField] private float agitatedLeapMultiplier = 1.45f;
    [Header("Leaf Throw State")]
    [SerializeField] private float leafChargeTime = 0.5f;
    [SerializeField] private GameObject leafProjectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int leafBurstCount = 2;
    [SerializeField] private float timeBetweenLeaves = 0.2f;
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    public MantisIdleState IdleState { get; private set; }
    public MantisSwipeState SwipeState { get; private set; }
    public MantisLeapState LeapState { get; private set; }
    public MantisLeafThrowState LeafThrowState { get; private set; }

    public float SpeedMultiplier => IsAgitated ? agitatedSpeedMultiplier : 1f;
    public float LeapAttackChance => leapAttackChance;
    public float SwipeAttackChance => swipeAttackChance;
    public float DecisionCooldown => decisionCooldown / SpeedMultiplier;

    public float MaxSwipeHeightDifference => maxSwipeHeightDifference;
    public float SwipeTriggerRange => swipeTriggerRange;
    public float SwipeChargeTime => swipeChargeTime / SpeedMultiplier;
    public float TimeBetweenSwipes => timeBetweenSwipes / SpeedMultiplier;
    public float SwipeLungeSpeed => swipeLungeSpeed * SpeedMultiplier;
    public int SwipeCount => swipeCount;
    public GameObject SwipeHitbox => swipeHitbox;

    public float LeapChargeTime => leapChargeTime / SpeedMultiplier;
    public float LeapHorizontalForce => leapHorizontalForce * (IsAgitated ? agitatedLeapMultiplier : 1f);
    public float LeapVerticalForce => leapVerticalForce;

    public float LeafChargeTime => leafChargeTime / SpeedMultiplier;
    public GameObject LeafProjectilePrefab => leafProjectilePrefab;
    public Transform FirePoint => firePoint;
    public int LeafBurstCount => leafBurstCount;
    public float TimeBetweenLeaves => timeBetweenLeaves / SpeedMultiplier;


    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();

        IdleState = new MantisIdleState(this, StateMachine);
        DormantState = new BossDormantState(this, StateMachine, IdleState);
        HitState = new BossHitState(this, StateMachine, IdleState, hitStunDuration);
        SwipeState = new MantisSwipeState(this, StateMachine);
        LeapState = new MantisLeapState(this, StateMachine);
        LeafThrowState = new MantisLeafThrowState(this, StateMachine);

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        swipeHitbox.SetActive(false);
    }

    protected override void Start()
    {
        base.Start();
        SetPrototypeColor(Color.darkBlue);
        StateMachine.Initialize(DormantState);
    }
    protected override void UpdateAgitationState(bool agitated)
    {
        base.UpdateAgitationState(agitated);
        if (!IsDead && StateMachine.CurrentState != DormantState)
        {
            RestoreDefaultColor();
        }
    }
    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    public void SetPrototypeColor(Color color)
    {
        spriteRenderer.color = color;
    }
    public void RestoreDefaultColor()
    {
        SetPrototypeColor(IsAgitated ? Color.red : Color.darkGreen);
    }
    public override void OnHitStateEnter()
    {
        SetPrototypeColor(Color.black);
    }

    public override void OnHitStateExit()
    {
        RestoreDefaultColor();
    }
    protected override void Die()
    {
        base.Die();
        if (swipeHitbox != null) swipeHitbox.SetActive(false);
        SetPrototypeColor(Color.gray);
        Debug.Log("[MantisBoss] Defeated!");
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, swipeTriggerRange);
    }
}