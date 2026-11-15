using System.ComponentModel;
using UnityEngine;

public class MantisLeafThrowState : BossState
{
    private MantisBoss mantis;
    private float timer;
    private int leavesRemaining;
    private bool isCharging;
    public MantisLeafThrowState(MantisBoss boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
        mantis = boss;
    }

    public override void Enter()
    {
        mantis.FaceTarget();
        isCharging = true;
        leavesRemaining = mantis.LeafBurstCount;
        timer = mantis.LeafChargeTime;

        mantis.SetPrototypeColor(Color.cyan);
        mantis.Rb.linearVelocity = new Vector2(0f, mantis.Rb.linearVelocity.y);
    }

    public override void Tick()
    {
        timer -= Time.deltaTime;

        if (isCharging)
        {
            if (timer <= 0f)
            {
                isCharging = false;
                FireLeaf();
            }
            return;
        }
        if (timer <= 0f)
        {
            if (leavesRemaining > 0)
            {
                FireLeaf();
            }
            else
            {
                stateMachine.ChangeState(mantis.IdleState);
            }
        }
    }
    private void FireLeaf()
    {
        leavesRemaining--;
        timer = mantis.TimeBetweenLeaves;
        mantis.FaceTarget();

        if (mantis.LeafProjectilePrefab != null && mantis.Target != null)
        {
            Vector3 spawnPos = mantis.FirePoint != null ? mantis.FirePoint.position : mantis.transform.position;
            Vector2 direction = (mantis.Target.position - spawnPos).normalized;

            GameObject leafObj = Object.Instantiate(mantis.LeafProjectilePrefab, spawnPos, Quaternion.identity);
            if (leafObj.TryGetComponent(out LeafProjectile projectile))
            {
                projectile.Initialize(direction, mantis.gameObject);
            }
        }
    }

    public override void Exit()
    {
        mantis.RestoreDefaultColor();
    }
}