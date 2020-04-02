﻿using UnityEngine;

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

        // When exiting follow stage, check if there is any other players
        // still inside detection circle 
        owner.detectionCircle = Physics2D.OverlapCircle(owner.AIObject.transform.position, owner.detectionRadius);

        if (owner.detectionCircle.tag == StringData.player)
        {
            owner.playerObject = owner.detectionCircle.gameObject;
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
            RemovePlayerObject(owner);
        }
    }

    private void RemovePlayerObject(AI owner)
    {
        owner.sqrCurrDistance = (owner.playerObject.transform.position - owner.AIObject.transform.position).sqrMagnitude;

        if (owner.sqrCurrDistance > owner.sqrMaxDistance)
        {
            owner.playerObject = null;
        }
    }
}
