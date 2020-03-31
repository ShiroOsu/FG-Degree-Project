using UnityEngine;

public class PatrolState : State<AI>
{
    private static PatrolState s_instance;

    private PatrolState()
    {
        if (s_instance != null)
        {
            return;
        }

        s_instance = this;
    }

    public static PatrolState stateInstance
    {
        get
        {
            if (s_instance == null)
            {
                new PatrolState();
            }
            return s_instance;
        }
    }

    public override void EnterState(AI owner)
    {
        Debug.Log("Entering PatrolState.");
    }

    public override void ExitState(AI owner)
    {
        Debug.Log("Exiting PatrolState.");
    }

    public override void UpdateState(AI owner)
    {
        // TEMP
        if (owner.switchState)
        {
            owner.stateMachine.ChangeState(FollowState.stateInstance);
        }
    }
}
