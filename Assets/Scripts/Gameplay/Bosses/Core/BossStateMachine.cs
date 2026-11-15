public class BossStateMachine
{
    public BossState CurrentState { get; private set; }

    public void Initialize(BossState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(BossState newState)
    {
        if (CurrentState == newState) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }

    public void FixedTick()
    {
        CurrentState?.FixedTick();
    }
}