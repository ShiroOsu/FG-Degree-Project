using UnityEngine;

[RequireComponent(typeof(BoxcastController))]
public class PlayerController : BoxcastController
{
    public void Move(Vector2 velocity)
    {
        collisionsInfo.Reset();
        collisionsInfo.velocityOld = velocity;
      
        if (velocity.sqrMagnitude > 0.001f)
        {
            transform.Translate(velocity);
        }
    }
}