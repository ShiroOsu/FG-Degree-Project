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

        if (owner.playerObject)
        {
            DetectNearbyPlayer(owner);
        }
    }

    public override void UpdateState(AI owner)
    {
        if (!owner.playerObject)
        {
            owner.stateMachine.ChangeState(PatrolState.stateInstance);
        }

        if (owner.playerObject) // To ensure that we don't call null reference after playerObject is set to null
        {
            FollowPlayer(owner);
        }
    }

    private void DetectNearbyPlayer(AI owner)
    {
        // When exiting follow state, check if there is any other players
        // still inside detection circle 
        owner.detectionCircle = Physics2D.OverlapCircle(owner.transform.position, owner.detectionRadius);

        if (owner.detectionCircle.CompareTag(StringData.player))
        {
            owner.playerObject = owner.detectionCircle.gameObject;
        }
    }

    private void FollowPlayer(AI owner)
    {
        // Update current distance from playerObject
        owner.sqrCurrDistance = (owner.playerObject.transform.position - owner.transform.position).sqrMagnitude;

        //Vector2 target = new Vector2(owner.playerObject.transform.position.x, 0f);

        // Move towards playerObjects location
        owner.transform.position = Vector2.MoveTowards(owner.transform.position, owner.playerObject.transform.position,
            owner.speed * Time.deltaTime);

        if (owner.sqrCurrDistance > owner.sqrMaxDistance)
        {
            owner.playerObject = null;
        }
    }
}
