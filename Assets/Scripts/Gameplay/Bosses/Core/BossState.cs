public abstract class BossState
{
    protected BossBase boss;
    protected BossStateMachine stateMachine;

    public BossState(BossBase boss, BossStateMachine stateMachine)
    {
        this.boss = boss;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void FixedTick() { }
    public virtual void Exit() { }
}