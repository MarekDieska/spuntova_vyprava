using UnityEngine;

public class BossDormantState : BossState
{
    private BossState nextCombatState;

    public BossDormantState(BossBase boss, BossStateMachine stateMachine, BossState nextCombatState) : base(boss, stateMachine)
    {
        this.nextCombatState = nextCombatState;
    }

    public override void Enter()
    {
        boss.Rb.linearVelocity = Vector2.zero;
    }

    public override void Tick()
    {
        if (boss.IsEncounterActive)
        {
            stateMachine.ChangeState(nextCombatState);
        }
    }
}