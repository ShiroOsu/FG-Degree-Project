using UnityEngine;

[RequireComponent(typeof(BoxcastController))]
public class PlayerController : BoxcastController
{
    public void Move(Vector2 velocity)
    {
        collisionsInfo.Reset();
        collisionsInfo.velocityOld = velocity;

        if (velocity.x != 0)
        {
            collisionsInfo.forwardDirection.x = (int)Mathf.Sign(velocity.x);
        }

        if (velocity.y != 0)
        {
            collisionsInfo.forwardDirection.y = (int)Mathf.Sign(velocity.y);
        }

        if (velocity.sqrMagnitude > 0.001f)
        {
            transform.Translate(velocity);
        }
    }
}