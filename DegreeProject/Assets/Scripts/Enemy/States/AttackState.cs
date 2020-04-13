using UnityEngine;

public class AttackState : MonoBehaviour, iState<AI>
{
    private float nextAttackTime = 0f;

    public void EnterState(AI owner)
    {
        owner.animator.SetInteger(StringData.animState, 1);
        Debug.Log("Enter AttackState");
    }

    public void ExitState(AI owner)
    {
        owner.animator.SetInteger(StringData.animState, 2);
        Debug.Log("Exiting AttackState");
    }

    public void UpdateState(AI owner)
    {
        if (owner.sqrCurrDistance > owner.attackRange)
        {
            owner.stateMachine.ChangeState(owner.followState);
        }

        if (Time.time >= nextAttackTime)
        {
            if (!owner.AnimationIsPlaying(StringData.attack) && !owner.AnimationIsPlaying(StringData.hurt))
            {
                nextAttackTime = Time.time + (1f / owner.attackRate);
                Attack(owner);
            }
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
                if (col.GetComponent<Player>().GetHP() <= 0f)
                {
                    owner.playerObject = null;
                }

                col.GetComponent<Player>().TakeDamage(owner.damage);
            }
        }
    }
}
