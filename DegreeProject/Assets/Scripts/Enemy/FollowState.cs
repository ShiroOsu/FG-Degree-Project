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
        }

        if (owner.playerObject) // To ensure that we don't call null reference after playerObject is set to null
        {
            FollowPlayer(owner);
        }
    }

    // Move functions should move to AI script probably.
    private void FollowPlayer(AI owner)
    {
        // Update current distance from playerObject
        owner.sqrCurrDistance = (owner.playerObject.transform.position - owner.transform.position).sqrMagnitude;

        // Move towards playerObjects location
        owner.transform.position = Vector2.MoveTowards(owner.transform.position, owner.playerObject.transform.position,
            owner.speed * Time.deltaTime);

        if (owner.sqrCurrDistance > owner.sqrMaxDistance)
        {
            owner.playerObject = null;
        }
    }
}
