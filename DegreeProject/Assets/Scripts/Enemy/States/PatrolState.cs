using UnityEngine;

public class PatrolState : iState<AI>
{
    private bool rightWall;
    private bool leftWall;
    private Vector2 direction;
    private Vector2 boxSize = new Vector2(0.15f, 1.15f);
    private string[] ground = { StringData.groundLayer };
    private string[] enemy = { StringData.enemyLayer };

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
        float distance = 0.15f;

        // Right 
        {
            Vector2 direction = Vector2.right;

            RaycastHit2D hit = Physics2D.BoxCast(originRight, boxSize, 0f, direction, distance, LayerMask.GetMask(ground));

            // On the right side of the enemy the distance of the ray needs to be longer because otherwise it wont hit
            RaycastHit2D hitE = Physics2D.Raycast(originRight, direction, 0.3f, LayerMask.GetMask(enemy));

            if (hit)
            {
                rightWall = true;
            }
            else { rightWall = false; }

            var otherEnemy = hitE.collider;

            if (otherEnemy != null)
            {
                if (otherEnemy.name != owner.name)
                {
                    rightWall = true;
                }
                else { rightWall = false; }
            }
        }

        // Left 
        {
            Vector2 direction = Vector2.left;

            RaycastHit2D hit = Physics2D.BoxCast(originLeft, boxSize, 0f, direction, distance, LayerMask.GetMask(ground));
            RaycastHit2D hitE = Physics2D.Raycast(originLeft, direction, distance, LayerMask.GetMask(enemy));

            if (hit)
            {
                leftWall = true;
            }
            else { leftWall = false; }

            var otherEnemy = hitE.collider;

            if (otherEnemy != null)
            {
                if (otherEnemy.name != owner.name)
                {
                    leftWall = true;
                }
                else { leftWall = false; }
            }
        }
    }
}
