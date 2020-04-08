[System.Serializable]
public class StateMachine<T>
{
    public iState<T> currentState { get; private set; }
    public T owner;

    public StateMachine(T owner)
    {
        this.owner = owner;
        currentState = null;
    }

    public void ChangeState(iState<T> newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(owner); 
        }

        currentState = newState;
        currentState.EnterState(owner);
    }

    public void UpdateState()
    {
        if (currentState != null)
        {
            currentState.UpdateState(owner);
        }
    }
}

public interface iState<T>
{
    void EnterState(T owner);
    void ExitState(T owner);
    void UpdateState(T owner);
}
