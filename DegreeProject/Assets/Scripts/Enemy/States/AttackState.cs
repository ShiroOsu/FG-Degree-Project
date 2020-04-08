using UnityEngine;

public class AttackState : MonoBehaviour, iState<AI>
{
    public void EnterState(AI owner)
    {
        Debug.Log("Enter AttackState");
    }

    public void ExitState(AI owner)
    {
        Debug.Log("Exiting AttackState");
    }

    public void UpdateState(AI owner)
    {
        if (owner.sqrCurrDistance > owner.attackRange)
        {
            owner.stateMachine.ChangeState(owner.followState);
        }

        if (!owner.AnimationIsPlaying(StringData.attack) && !owner.AnimationIsPlaying(StringData.hurt))
        {
            Attack(owner);
        }

        if (!owner.playerObject) // Player died maybe
        {
            owner.stateMachine.ChangeState(owner.patrolState);
        }
    }

    private void Attack(AI owner)
    {
        owner.AttackAnimation();

        Collider2D[] hitByAttack = Physics2D.OverlapCircleAll(owner.attackPoint.position, owner.attackRange, LayerMask.GetMask(StringData.playerLayer));

        foreach (var col in hitByAttack)
        {
            if (col.GetComponent<Player>() != null)
            {
                col.GetComponent<Player>().TakeDamage(owner.damage);
            }
        }
    }
}
