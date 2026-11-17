using System.Collections.Generic;
using UnityEngine;

public class MantisLeapState : BossState
{
    private MantisBoss mantis;
    private Collider2D bossCollider;
    private PlatformEffector2D[] oneWayEffectors;
    private readonly List<Collider2D> ignoredPlatforms = new List<Collider2D>();

    private float timer;
    private bool hasLaunched;
    private float airTime;
    private float stationaryTimer;
    private float targetFeetY;

    private const float MaxAirTime = 2.5f;

    public MantisLeapState(MantisBoss boss, BossStateMachine stateMachine) : base(boss, stateMachine)
    {
        mantis = boss;
        bossCollider = boss.GetComponent<Collider2D>();
    }

    public override void Enter()
    {
        if (oneWayEffectors == null)
        {
            oneWayEffectors = Object.FindObjectsByType<PlatformEffector2D>(FindObjectsSortMode.None);
        }

        mantis.FaceTarget();
        hasLaunched = false;
        airTime = 0f;
        stationaryTimer = 0f;
        timer = mantis.LeapChargeTime;

        mantis.SetPrototypeColor(Color.orange);
        mantis.Rb.linearVelocity = new Vector2(0f, mantis.Rb.linearVelocity.y);
    }

    public override void Tick()
    {
        if (!hasLaunched)
        {
            mantis.FaceTarget();
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Launch();
            }
            return;
        }

        airTime += Time.deltaTime;

        if (ignoredPlatforms.Count > 0 && airTime > 0.12f && mantis.Rb.linearVelocity.y <= 0f)
        {
            float bossFeetY = bossCollider.bounds.min.y;
            if (bossFeetY <= targetFeetY + 0.35f)
            {
                RestoreIgnoredPlatforms();
            }
        }

        if (Mathf.Abs(mantis.Rb.linearVelocity.y) < 0.05f)
        {
            stationaryTimer += Time.deltaTime;
        }
        else
        {
            stationaryTimer = 0f;
        }

        if (airTime > 0.15f && ignoredPlatforms.Count == 0)
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

        IgnoreAllOneWayPlatforms();

        float dir = mantis.IsFacingRight ? 1f : -1f;

        Vector2 startPos = mantis.transform.position;
        Vector2 targetPos = mantis.Target.position;
        float dx = targetPos.x - startPos.x;
        float dy = targetPos.y - startPos.y;

        if (mantis.Target.TryGetComponent(out Collider2D targetCol))
            targetFeetY = targetCol.bounds.min.y;
        else
            targetFeetY = targetPos.y - 1.5f;

        if (Mathf.Abs(dx) > mantis.DashLeapDistance)
        {
            float gravity = Mathf.Abs(Physics2D.gravity.y * mantis.Rb.gravityScale);
            if (gravity < 0.01f) gravity = 9.81f;

            Vector2 toTarget = targetPos - startPos;
            float distance = Mathf.Max(0.1f, toTarget.magnitude);
            float flightTime = distance / mantis.DashLeapSpeed;

            float vx = dx / flightTime;
            float vy = (dy / flightTime) + (0.5f * gravity * flightTime);

            mantis.Rb.linearVelocity = new Vector2(vx, vy);
            return;
        }

        mantis.Rb.linearVelocity = new Vector2(dir * mantis.LeapHorizontalForce, mantis.LeapVerticalForce);
    }

    private void IgnoreAllOneWayPlatforms()
    {

        for (int i = 0; i < oneWayEffectors.Length; i++)
        {
            PlatformEffector2D effector = oneWayEffectors[i];

            Collider2D[] cols = effector.GetComponents<Collider2D>();
            for (int j = 0; j < cols.Length; j++)
            {
                Collider2D col = cols[j];
                if (!col.isTrigger && !ignoredPlatforms.Contains(col))
                {
                    Physics2D.IgnoreCollision(bossCollider, col, true);
                    ignoredPlatforms.Add(col);
                }
            }
        }
    }

    private void RestoreIgnoredPlatforms()
    {
        if (bossCollider == null)
        {
            ignoredPlatforms.Clear();
            return;
        }

        for (int i = ignoredPlatforms.Count - 1; i >= 0; i--)
        {
            Collider2D col = ignoredPlatforms[i];
            if (col == null)
            {
                ignoredPlatforms.RemoveAt(i);
            }
            else if (!bossCollider.Distance(col).isOverlapped)
            {
                Physics2D.IgnoreCollision(bossCollider, col, false);
                ignoredPlatforms.RemoveAt(i);
            }
        }
    }

    public override void Exit()
    {
        if (bossCollider != null)
        {
            for (int i = 0; i < ignoredPlatforms.Count; i++)
            {
                if (ignoredPlatforms[i] != null)
                {
                    Physics2D.IgnoreCollision(bossCollider, ignoredPlatforms[i], false);
                }
            }
        }
        ignoredPlatforms.Clear();

        if (!mantis.IsAgitated)
        {
            mantis.SetVulnerable(false);
        }
        mantis.RestoreDefaultColor();
        mantis.Rb.linearVelocity = new Vector2(0f, mantis.Rb.linearVelocity.y);
    }
}