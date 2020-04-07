#define DEBUG

using UnityEngine;
using Mirror;

public class AI : NetworkBehaviour
{
    [Header("AI Settings")]
    [SerializeField] [SyncVar] private float health = 10f;
    [SerializeField] private float damage = 1f;
    public float speed = 2f;

    // AI components
    public Collider2D detectionCircle { get; set; }
    public GameObject playerObject { get; set; }

    [Header("Detection Settings")]
    [SerializeField] private float maxDetectionDistance = 8f;
    public float detectionRadius = 5f;
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

#if DEBUG
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(gameObject.transform.position, detectionRadius);
        
        Gizmos.DrawWireCube(new Vector3(transform.position.x - 0.5f, transform.position.y + 0.65f, 0f), new Vector3(0.15f, 1.15f, 0f));
        Gizmos.DrawWireCube(new Vector3(transform.position.x + 0.5f, transform.position.y + 0.65f, 0f), new Vector3(0.15f, 1.15f, 0f));
    }
#endif
}
