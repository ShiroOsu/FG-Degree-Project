using UnityEngine;

public class FollowState : MonoBehaviour, iState<AI>
{
    public void EnterState(AI owner)
    {
        Debug.Log("Entering FollowState.");
    }

    public void ExitState(AI owner)
    {
        Debug.Log("Exiting FollowState.");
    }

    public void UpdateState(AI owner)
    {
        if (!owner.playerObject)
        {
            owner.stateMachine.ChangeState(owner.patrolState);
            return;
        }

        if (owner.playerObject) // To ensure that we don't call null reference after playerObject is set to null
        {
            FollowPlayer(owner);
        }
    }

    // Move functions should move to AI script probably.
    private void FollowPlayer(AI owner)
    {
        // Stop in front of player
        if (owner.sqrCurrDistance > 1f)
        {
            owner.FollowMove();
        } else
        {
            owner.stateMachine.ChangeState(owner.attackState);
            return;
        }

        if (owner.sqrCurrDistance > owner.sqrMaxDistance)
        {
            owner.playerObject = null;
        }
    }
}
