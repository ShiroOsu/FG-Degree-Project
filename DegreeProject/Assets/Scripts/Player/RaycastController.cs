using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    protected const float skinWidth = 0.015f;
    public int horizontalRayCount { get { return 4; } set { } }
    public int verticalRayCount { get { return 4; } set { } }

    public LayerMask collisionMask;

    public float horizontalRaySpacing
    {
        get;
        private set;
    }

    public float verticalRaySpacing
    {
        get;
        private set;
    }

    public BoxCollider2D boxCollider
    {
        get;
        private set;
    }

    [HideInInspector] public RaycastOrigins raycastCorners;

    public virtual void Start()
    {
        if (!boxCollider)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }

        CalculateRaySpacing();
    }

    public void UpdateRaycasts()
    {
        Bounds boxBounds = boxCollider.bounds;
        boxBounds.Expand(skinWidth * -2);

        raycastCorners.topLeft = new Vector2(boxBounds.min.x, boxBounds.max.y);
        raycastCorners.topRight = new Vector2(boxBounds.max.x, boxBounds.max.y);
        
        raycastCorners.bottomLeft = new Vector2(boxBounds.min.x, boxBounds.min.y);
        raycastCorners.bottomRight = new Vector2(boxBounds.max.x, boxBounds.min.y);
    }

    private void CalculateRaySpacing()
    {
        Bounds boxBounds = boxCollider.bounds;
        boxBounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = boxBounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = boxBounds.size.x / (verticalRayCount - 1);
    }
}
