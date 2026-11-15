using UnityEngine;

public class MantisLeapState : BossState
{
    private MantisBoss mantis;
    private float timer;
    private bool hasLaunched;
    private float airTime;
    private float stationaryTimer;

    private const float MaxAirTime = 2.5f;

    public MantisLeapState(MantisBoss boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
        mantis = boss;
    }

    public override void Enter()
    {
        mantis.FaceTarget();
        hasLaunched = false;
        airTime = 0f;
        timer = mantis.LeapChargeTime;

        mantis.SetPrototypeColor(Color.orange);
        mantis.Rb.linearVelocity = new Vector2(0f, mantis.Rb.linearVelocity.y);
    }

    public override void Tick()
    {
        if (!hasLaunched)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Launch();
            }
            return;
        }

        airTime += Time.deltaTime;

        if (Mathf.Abs(mantis.Rb.linearVelocity.y) < 0.05f)
        {
            stationaryTimer += Time.deltaTime;
        }
        else
        {
            stationaryTimer = 0f;
        }

        if (airTime > 0.15f)
        {
            bool landed = mantis.IsGrounded();
            bool stalled = stationaryTimer >= 0.2f;
            bool timedOut = airTime >= MaxAirTime;

            if (landed || stalled || timedOut)
            {
                stateMachine.ChangeState(mantis.IdleState);
            }
        }
    }

    private void Launch()
    {
        hasLaunched = true;
        mantis.SetPrototypeColor(Color.beige);

        mantis.SetVulnerable(true);

        float dir = mantis.IsFacingRight ? 1f : -1f;
        mantis.Rb.linearVelocity = new Vector2(dir * mantis.LeapHorizontalForce, mantis.LeapVerticalForce);
    }

    public override void Exit()
    {
        if (!mantis.IsAgitated)
        {
            mantis.SetVulnerable(false);
        }
        mantis.RestoreDefaultColor();
        mantis.Rb.linearVelocity = new Vector2(0f, mantis.Rb.linearVelocity.y);
    }
}