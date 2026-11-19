using UnityEngine;

public class BossHitState : BossState
{
    private BossState nextState;
    private float stunDuration;
    private float timer;

    public BossHitState(BossBase boss, BossStateMachine stateMachine, BossState nextState, float stunDuration = 0.75f)
        : base(boss, stateMachine)
    {
        this.nextState = nextState;
        this.stunDuration = stunDuration;
    }

    public override void Enter()
    {
        timer = stunDuration;
        boss.Rb.linearVelocity = new Vector2(0f, boss.Rb.linearVelocity.y);
        boss.SetHitInvulnerable(true);
        boss.OnHitStateEnter();
    }

    public override void Tick()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f && nextState != null)
        {
            stateMachine.ChangeState(nextState);
        }
    }

    public override void Exit()
    {
        boss.SetHitInvulnerable(false);
        boss.OnHitStateExit();
    }
}