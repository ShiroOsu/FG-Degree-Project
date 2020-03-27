#define DEBUG
#undef DEBUG

using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BoxcastController : MonoBehaviour
{
    public struct CollisionsInfo
    {
        public bool above, below;
        public bool left, right;

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }

    private struct BoxcastOrigins
    {
        public Vector2 horizontalRayOrigin, verticalRayOrigin;
        public Vector2 horizontalRaySize, verticalRaySize;
    }

    private BoxCollider2D boxCollider;
    private BoxcastOrigins boxcastOrigins;

    public CollisionsInfo collisionsInfo;
    public LayerMask collisionMask;

    protected const float skinWidth = 0.015f;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void UpdateCollision(Vector2 velocity, in Vector2 directionalInput)
    {
        HorizontalCollision(in velocity, in directionalInput);
        VerticalCollision(in velocity, in directionalInput);

#if DEBUG
        DrawCollisionChecks();
#endif
    }

    private void HorizontalCollision(in Vector2 velocity, in Vector2 directionalInput)
    {
        float directionX = velocity.x != 0 ? Mathf.Sign(velocity.x) : directionalInput.x != 0 ? Mathf.Sign(directionalInput.x) : 1;
        float rayDistance = boxCollider.bounds.extents.x;

        boxcastOrigins.horizontalRayOrigin = new Vector2
            (boxCollider.bounds.center.x + (boxCollider.bounds.extents.x * 0.5f + skinWidth) * directionX, boxCollider.bounds.center.y);
        
        boxcastOrigins.horizontalRaySize = new Vector2(rayDistance, boxCollider.bounds.extents.y);

        RaycastHit2D hit = Physics2D.BoxCast(boxcastOrigins.horizontalRayOrigin, boxcastOrigins.horizontalRaySize,
            0f, Vector2.zero, rayDistance, collisionMask);

        if (hit)
        {
            collisionsInfo.right = directionX == 1;
            collisionsInfo.left = directionX == -1;
        }
    }

    private void VerticalCollision(in Vector2 velocity, in Vector2 directionalInput)
    {
        float directionY = velocity.y != 0 ? Mathf.Sign(velocity.y) : directionalInput.y != 0 ? Mathf.Sign(directionalInput.y) : -1;
        float rayDistance = boxCollider.bounds.extents.y;

        boxcastOrigins.verticalRayOrigin = new Vector2
            (boxCollider.bounds.center.x, boxCollider.bounds.center.y + (boxCollider.bounds.extents.y * 0.5f + skinWidth) * directionY);
        
        boxcastOrigins.verticalRaySize = new Vector2(boxCollider.bounds.extents.x, rayDistance);

        RaycastHit2D hit = Physics2D.BoxCast(boxcastOrigins.verticalRayOrigin, boxcastOrigins.verticalRaySize, 
            0f, Vector2.zero, rayDistance, collisionMask);

        if (hit)
        {
            collisionsInfo.above = directionY == 1;
            collisionsInfo.below = directionY == -1;
        }
    }

#if DEBUG
    Vector3[] horizontalVerts = new Vector3[4];
    Vector3[] verticalVerts = new Vector3[4];

    private void DrawCollisionChecks()
    {
        if (boxcastOrigins.horizontalRayOrigin != Vector2.zero)
        {
            horizontalVerts[0] = new Vector3(boxcastOrigins.horizontalRayOrigin.x - boxcastOrigins.horizontalRaySize.x * 0.5f, boxcastOrigins.horizontalRayOrigin.y + boxcastOrigins.horizontalRaySize.y * 0.5f, 0);
            horizontalVerts[1] = new Vector3(boxcastOrigins.horizontalRayOrigin.x + boxcastOrigins.horizontalRaySize.x * 0.5f, boxcastOrigins.horizontalRayOrigin.y + boxcastOrigins.horizontalRaySize.y * 0.5f, 0);
            horizontalVerts[2] = new Vector3(boxcastOrigins.horizontalRayOrigin.x + boxcastOrigins.horizontalRaySize.x * 0.5f, boxcastOrigins.horizontalRayOrigin.y - boxcastOrigins.horizontalRaySize.y * 0.5f, 0);
            horizontalVerts[3] = new Vector3(boxcastOrigins.horizontalRayOrigin.x - boxcastOrigins.horizontalRaySize.x * 0.5f, boxcastOrigins.horizontalRayOrigin.y - boxcastOrigins.horizontalRaySize.y * 0.5f, 0);

            for (int i = 0; i < 3; i++)
            {
                Debug.DrawLine(horizontalVerts[i], horizontalVerts[i + 1]);
            }
            Debug.DrawLine(horizontalVerts[3], horizontalVerts[0]);
        }

        if (boxcastOrigins.verticalRayOrigin != Vector2.zero)
        {

            verticalVerts[0] = new Vector3(boxcastOrigins.verticalRayOrigin.x - boxcastOrigins.verticalRaySize.x * 0.5f, boxcastOrigins.verticalRayOrigin.y + boxcastOrigins.verticalRaySize.y * 0.5f, 0);
            verticalVerts[1] = new Vector3(boxcastOrigins.verticalRayOrigin.x + boxcastOrigins.verticalRaySize.x * 0.5f, boxcastOrigins.verticalRayOrigin.y + boxcastOrigins.verticalRaySize.y * 0.5f, 0);
            verticalVerts[2] = new Vector3(boxcastOrigins.verticalRayOrigin.x + boxcastOrigins.verticalRaySize.x * 0.5f, boxcastOrigins.verticalRayOrigin.y - boxcastOrigins.verticalRaySize.y * 0.5f, 0);
            verticalVerts[3] = new Vector3(boxcastOrigins.verticalRayOrigin.x - boxcastOrigins.verticalRaySize.x * 0.5f, boxcastOrigins.verticalRayOrigin.y - boxcastOrigins.verticalRaySize.y * 0.5f, 0);

            for (int i = 0; i < 3; i++)
            {
                Debug.DrawLine(verticalVerts[i], verticalVerts[i + 1]);
            }
            Debug.DrawLine(verticalVerts[3], verticalVerts[0]);
        }
    }
#endif
}
