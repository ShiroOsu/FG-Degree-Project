using UnityEngine;

public class PlayerController : RaycastController
{
    public struct CollisionInfo
    {
        public bool above, under;
        public bool left, right;

        public int forwardDirection;
        public int upwardsDirection;

        public Vector3 velocityOld;

        public void Reset()
        {
            above = under = false;
            left = right = false;
        }
    }

    public CollisionInfo collisionInfo;

    public override void Start()
    {
        base.Start();
        collisionInfo.forwardDirection = 1;
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycasts();
        collisionInfo.Reset();
        collisionInfo.velocityOld = velocity;

        if (velocity.x != 0)
        {
            collisionInfo.forwardDirection = (int)Mathf.Sign(velocity.x);
        }

        if (velocity.y != 0)
        {
            collisionInfo.upwardsDirection = (int)Mathf.Sign(velocity.y);
        }

        VerticalCollision(ref velocity);
        HorizontalCollision(ref velocity);

        transform.Translate(velocity);
    }

    private void HorizontalCollision(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        if (Mathf.Abs(velocity.x) < skinWidth)
        {
            rayLength = skinWidth * 2;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastCorners.bottomLeft : raycastCorners.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D raycastHit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.green);

            if (raycastHit)
            {
                rayLength = raycastHit.distance;

                collisionInfo.left = directionX == -1;
                collisionInfo.right = directionX == 1;
            }
        }
    }

    private void VerticalCollision(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastCorners.bottomLeft : raycastCorners.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D raycastHit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (raycastHit)
            {
                rayLength = raycastHit.distance;

                collisionInfo.under = directionY == -1;
                collisionInfo.above = directionY == 1;
            }

            // Seems like the ray cast gets to long when landing from a high position,
            // and resetting the length of the ray doesn't seem to help.
            // So when the ray cast does not hit, and the ray is incredibly long
            // while collision under the player is false, set it to true.
            if (!raycastHit && rayLength > 0.3f && !collisionInfo.under)
            {
                collisionInfo.under = true;
            }
        }
    }
}
