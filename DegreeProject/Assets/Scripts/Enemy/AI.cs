using UnityEngine;
using Mirror;

public class AI : NetworkBehaviour
{
    [Header("Health & Damage")]
    [SerializeField] [SyncVar] private float health;
    [SerializeField] private float damage;

    [SerializeField] private float speed;

    public bool switchState { get; set; } // Switching between Patrol and Follow state.

    public StateMachine<AI> stateMachine { get; set; }

    private void Start()
    {
        stateMachine = new StateMachine<AI>(this);
        stateMachine.ChangeState(PatrolState.s_stateInstance);
    }

    private void Update()
    {
        stateMachine.UpdateState();
    }

    private void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            DestroyIfDead(true);
        }
    }

    private void DestroyIfDead(bool dead)
    {
        if (dead)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

}
