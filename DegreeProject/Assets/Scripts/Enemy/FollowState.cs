using UnityEngine;

public class FollowState : State<AI>
{
    private static FollowState s_instance;

    private FollowState()
    {
        if (s_instance != null)
        {
            return;
        }

        s_instance = this;
    }

    public static FollowState stateInstance
    {
        get
        {
            if (s_instance == null)
            {
                new FollowState();
            }
            return s_instance;
        }
    }

    public override void EnterState(AI owner)
    {
        Debug.Log("Entering FollowState.");
    }

    public override void ExitState(AI owner)
    {
        Debug.Log("Exiting FollowState.");
    }

    public override void UpdateState(AI owner)
    {
        if (!owner.switchState)
        {
            owner.stateMachine.ChangeState(PatrolState.stateInstance);
        }
    }
}
