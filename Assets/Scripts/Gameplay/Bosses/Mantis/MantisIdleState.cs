using UnityEngine;
public class MantisIdleState : BossState
{
    private MantisBoss mantis;
    private float timer;

    public MantisIdleState(MantisBoss boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
        mantis = boss;
    }

    public override void Enter()
    {
        timer = mantis.DecisionCooldown;
        mantis.RestoreDefaultColor();
        mantis.Rb.linearVelocity = new Vector2(0f, mantis.Rb.linearVelocity.y);
    }

    public override void Tick()
    {
        if (mantis.Target == null) return;
        mantis.FaceTarget();

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ChooseNextAttack();
        }
    }
    private void ChooseNextAttack()
    {
        float distance = mantis.DistanceToTarget();
        float heightDifference = mantis.Target.position.y - mantis.transform.position.y;

        if (heightDifference > mantis.MaxSwipeHeightDifference)
        {
            stateMachine.ChangeState(mantis.LeapState);
            return;
        }

        if (distance <= mantis.SwipeTriggerRange)
        {
            if (Random.value < mantis.SwipeAttackChance)
            {
                stateMachine.ChangeState(mantis.SwipeState);
            }
            else
            {
                stateMachine.ChangeState(mantis.LeapState);
            }
        }
        else
        {
            if (Random.value < mantis.LeapAttackChance)
            {
                stateMachine.ChangeState(mantis.LeapState);
            }
            else
            {
                stateMachine.ChangeState(mantis.LeafThrowState);
            }
        }
    }
}