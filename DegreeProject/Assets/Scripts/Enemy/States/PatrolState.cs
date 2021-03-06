﻿using UnityEngine;

public class PatrolState : iState<AI>
{
    private bool rightWall;
    private bool leftWall;
    private Vector2 direction;
    private Vector2 boxSize = new Vector2(0.15f, 1.15f);
    private float distance = 0.15f;

    public void EnterState(AI owner)
    {
        // When spawning a AI in an open Area it will not know if it
        // should start walking to the left or right
        if (direction.x == 0f)
        {
            System.Random rand = new System.Random();
            direction = rand.Next(2) == 1 ? Vector2.right : Vector2.left;
        }

        Debug.Log("Entering PatrolState.");
    }

    public void ExitState(AI owner)
    {
        Debug.Log("Exiting PatrolState.");
    }

    public void UpdateState(AI owner)
    {
        if (owner.playerObject)
        {
            owner.stateMachine.ChangeState(owner.followState);
            return;
        }

        if (!owner.playerObject)
        {
            DetectNearbyPlayer(owner);
        }

        Patrol(owner);
    }

    private void DetectNearbyPlayer(AI owner)
    {
        owner.detectionCircle = Physics2D.OverlapCircleAll(owner.transform.position, owner.detectionRadius);

        foreach (var col in owner.detectionCircle)
        {
            var player = col.GetComponent<Player>();

            if (player != null && player.GetHP > 0f)
            {
                owner.playerObject = player.gameObject;
                return;
            }
        }
    }

    private void Patrol(AI owner)
    {
        HorizontalCollision(owner);

        if (rightWall)
        {
            direction = Vector2.left;
        }
        else if (leftWall)
        {
            direction = Vector2.right;
        }

        owner.SetDirection(direction);
        owner.PatrolMove(direction);
    }

    private void HorizontalCollision(AI owner)
    {
        Vector2 originRight = new Vector2(owner.transform.position.x + 0.5f, owner.transform.position.y + 0.65f);
        Vector2 originLeft = new Vector2(owner.transform.position.x - 0.5f, owner.transform.position.y + 0.65f);
        Vector2 origin = direction.x == 1 ? originRight : originLeft;

        RayAndBoxCast(owner, origin, direction, boxSize, distance);
    }

    private void RayAndBoxCast(AI owner, Vector2 origin, Vector2 direction, Vector2 boxSize, float distance)
    {
        // Don't know why but the 'right' ray-cast distance needs to be longer.
        if (direction.x == 1)
            distance = 0.3f;

        RaycastHit2D wallHit = Physics2D.BoxCast(origin, boxSize, 0f, direction, distance, owner.groundMask);
        RaycastHit2D enemyHit = Physics2D.Raycast(origin, direction, distance, owner.enemyMask);

        if (wallHit)
        {
            rightWall = direction.x == 1;
            leftWall = direction.x == -1;
        }

        if (enemyHit.collider != null)
        {
            var enemyID = enemyHit.collider.gameObject.GetInstanceID();
            var thisID = owner.gameObject.GetInstanceID();

            rightWall = enemyID != thisID && direction.x == 1;
            leftWall = enemyID != thisID && direction.x == -1;
        }
    }
}
