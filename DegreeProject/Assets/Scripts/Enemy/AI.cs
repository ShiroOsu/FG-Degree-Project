using UnityEngine;
using Mirror;

public class AI : NetworkBehaviour
{
    [Header("Health & Damage")]
    [SerializeField] [SyncVar] private float health = 1f;
    [SerializeField] private float damage = 1f;

    [SerializeField] private float speed = 1f;

    public bool switchState { get; set; } // Switching between Patrol and Follow state.

    public StateMachine<AI> stateMachine { get; set; }

    private void Start()
    {
        stateMachine = new StateMachine<AI>(this);
        stateMachine.ChangeState(PatrolState.stateInstance);
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
