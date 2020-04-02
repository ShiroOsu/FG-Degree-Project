using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class AI : NetworkBehaviour
{
    [Header("Health & Damage")]
    [SerializeField] [SyncVar] private float health = 1f;
    [SerializeField] private float damage = 1f;

    [SerializeField] private float speed = 1f;

    // AI components
    public Collider2D detectionCircle { get; set; }
    public GameObject playerObject { get; set; }
    public GameObject AIObject { get; private set; }

    [SerializeField] private float maxDetectionDistance = 8f;
    public float sqrCurrDistance { get; set; }
    public float sqrMaxDistance
    { 
        get 
        { 
            return maxDetectionDistance * maxDetectionDistance; 
        } 
    }

    public StateMachine<AI> stateMachine { get; private set; }

    private void Start()
    {
        AIObject = gameObject;

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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(gameObject.transform.position, radius:5f);
    }
}
