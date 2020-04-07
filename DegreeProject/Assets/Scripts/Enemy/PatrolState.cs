using UnityEngine;

public class PatrolState : State<AI>
{
    private static PatrolState s_instance;
    private bool rightWall;
    private bool leftWall;
    private Vector2 direction;
    private Vector2 boxSize = new Vector2(0.15f, 1.15f);

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
        if (owner.playerObject)
        {
            owner.stateMachine.ChangeState(FollowState.stateInstance);
        }

        if (!owner.playerObject)
        {
            DetectNearbyPlayer(owner);
        }

        Patrol(owner);
    }

    private void DetectNearbyPlayer(AI owner)
    {
        owner.detectionCircle = Physics2D.OverlapCircle(owner.transform.position, owner.detectionRadius);

        if (owner.detectionCircle.CompareTag(StringData.player))
        {
            owner.playerObject = owner.detectionCircle.gameObject;
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

        owner.transform.Translate(direction * owner.speed * Time.deltaTime);
    }

    private void HorizontalCollision(AI owner)
    {
        Vector2 originRight = new Vector2(owner.transform.position.x + 0.35f, owner.transform.position.y + 0.65f);
        Vector2 originLeft = new Vector2(owner.transform.position.x - 0.35f, owner.transform.position.y + 0.65f);
        float distance = 0.15f;

        // Right 
        {
            Vector2 direction = Vector2.right;

            RaycastHit2D hit = Physics2D.BoxCast(originRight, boxSize, 0, direction, distance, LayerMask.GetMask(StringData.ground));

            if (hit)
            {
                rightWall = true;
            } else { rightWall = false; }
        }

        // Left 
        {
            Vector2 direction = Vector2.left;

            RaycastHit2D hit = Physics2D.BoxCast(originLeft, boxSize, 0, direction, distance, LayerMask.GetMask(StringData.ground));

            if (hit)
            {
                leftWall = true;
            } else { leftWall = false; }
        }
    }
}
