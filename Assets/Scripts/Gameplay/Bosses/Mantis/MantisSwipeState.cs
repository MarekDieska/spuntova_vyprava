using System.ComponentModel;
using UnityEngine;
public class MantisSwipeState : BossState
{
    private MantisBoss mantis;
    private float timer;
    private int swipesRemaining;
    private bool isCharging;
    private bool hitboxActive;

    public MantisSwipeState(MantisBoss boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
        mantis = boss;
    }

    public override void Enter()
    {
        mantis.FaceTarget();
        isCharging = true;
        hitboxActive = false;
        swipesRemaining = mantis.SwipeCount;
        timer = mantis.SwipeChargeTime;

        mantis.SetPrototypeColor(Color.yellow);
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
                TriggerSingleSwipe();
            }
            return;
        }
        if (timer <= 0f)
        {
            if (hitboxActive)
            {
                hitboxActive = false;
                mantis.SwipeHitbox.SetActive(false);
                mantis.RestoreDefaultColor();
                mantis.Rb.linearVelocity = new Vector2(0f, mantis.Rb.linearVelocity.y);

                swipesRemaining--;
                if (swipesRemaining > 0)
                {
                    timer = mantis.TimeBetweenSwipes;
                }
                else
                {
                    stateMachine.ChangeState(mantis.IdleState);
                }
            }
            else
            {
                mantis.FaceTarget();
                TriggerSingleSwipe();
            }
        }
    }
    private void TriggerSingleSwipe()
    {
        hitboxActive = true;
        timer = mantis.TimeBetweenSwipes;
        mantis.SetPrototypeColor(Color.white);

        mantis.SwipeHitbox.SetActive(true);

        float dir = mantis.IsFacingRight ? 1f : -1f;
        mantis.Rb.linearVelocity = new Vector2(dir * mantis.SwipeLungeSpeed, mantis.Rb.linearVelocity.y);
    }

    public override void Exit()
    {
        if (mantis.SwipeHitbox != null) mantis.SwipeHitbox.SetActive(false);
        mantis.Rb.linearVelocity = new Vector2(0f, mantis.Rb.linearVelocity.y);
        mantis.RestoreDefaultColor();
    }
}