using UnityEngine;

public class AttackState : iState<AI>
{
    private float nextAttackTime = 0f;

    public void EnterState(AI owner)
    {
        owner.RpcAnimState(AI.AnimState.combat);
        Debug.Log("Enter AttackState");
    }

    public void ExitState(AI owner)
    {
        owner.RpcAnimState(AI.AnimState.run);
        Debug.Log("Exiting AttackState");
    }

    public void UpdateState(AI owner)
    {
        if (owner.sqrCurrDistance > owner.attackRange)
        {
            owner.stateMachine.ChangeState(owner.followState);
            return;
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
            return;
        }
    }

    private void Attack(AI owner)
    {
        owner.RpcAttackAnimation();

        var point = owner.dir.x > 0f ? owner.attackPointRight.position : owner.attackPointLeft.position;

        Collider2D[] hitByAttack = Physics2D.OverlapCircleAll(point, owner.attackRange, LayerMask.GetMask(StringData.playerLayer));

        foreach (var col in hitByAttack)
        {
            var player = col.GetComponent<Player>();

            if (player != null)
            {
                if (player.GetHP <= 0f)
                {
                    owner.playerObject = null;
                }

                player.TakeDamage(owner.damage);
            }
        }
    }
}
