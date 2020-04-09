#define DEBUG

using UnityEngine;
using Mirror;
using System.Collections;

public class AI : NetworkBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float health = 10f;
    [SyncVar] private float currentHealth;
    [SerializeField] private float speed = 2f;

    public float damage = 1f;
    public Transform attackPoint;
    public float attackRange = 1f;

    private Vector2 dir;

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

    // AI components
    public Collider2D[] detectionCircle { get; set; }
    public GameObject playerObject { get; set; }
    public StateMachine<AI> stateMachine { get; private set; }
    public FollowState followState { get; private set; }
    public PatrolState patrolState { get; private set; }
    public AttackState attackState { get; private set; }
    public Animator animator { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }

    private void Start()
    {
        followState = GetComponent<FollowState>();
        patrolState = GetComponent<PatrolState>();
        attackState = GetComponent<AttackState>();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        stateMachine = new StateMachine<AI>(this);
        stateMachine.ChangeState(patrolState);

        currentHealth = health;
    }

    private void Update()
    {
        if (playerObject != null)
        {
            sqrCurrDistance = (playerObject.transform.position - transform.position).sqrMagnitude;
        }

        stateMachine.UpdateState();

        FlipSpriteX();
    }

    public void PatrolMove(Vector2 direction)
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void FollowMove()
    {
        // Move towards playerObjects location (x axis)
        transform.position = Vector2.MoveTowards(transform.position,
            new Vector2(playerObject.transform.position.x, transform.position.y), speed * Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        StartCoroutine(HurtAnimation(0.5f));

        if (currentHealth <= 0f)
        {
            animator.StopPlayback(); // Still wont play death animation, it just pops
            StartCoroutine(Die(1f));
        }
    }

    private IEnumerator HurtAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger(StringData.hurt);
    }

    private IEnumerator Die(float delay)
    {
        if (gameObject != null)
        {
            animator.SetTrigger(StringData.death);
            yield return new WaitForSeconds(delay);
            NetworkServer.Destroy(gameObject);
        }
    }

    public bool AnimationIsPlaying(string state)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(state);
    }

    public void AttackAnimation()
    {
        animator.SetTrigger(StringData.attack);
    }

    public void SetDirection(Vector2 direction)
    {
        dir = direction;
        Vector2 right = new Vector2(1f, 1.5f);
        Vector2 left = new Vector2(-1f, 1.5f);

        if (dir.x > 0f)
        {
            attackPoint.localPosition = right;
        }
        else
        {
            attackPoint.localPosition = left;
        }
    }

    private void FlipSpriteX()
    {
        if (playerObject != null)
        {
            Vector2 playerDir = playerObject.GetComponent<Player>().GetDirInput();
            
            if (playerDir.x < 0f)
            {
                SetDirection((-1) * playerDir);
            }
            else if (playerDir.x > 0f)
            {
                SetDirection((-1) * playerDir);
            }
        }

        if (Mathf.Abs(dir.x) > Mathf.Epsilon)
        {
            animator.SetInteger(StringData.animState, 2);

            if (dir.x > 0f)
            {
                spriteRenderer.flipX = true;
            }
            else if (dir.x < 0f)
            {
                spriteRenderer.flipX = false;
            }
        }
    }


#if DEBUG
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(gameObject.transform.position, detectionRadius);
        Gizmos.DrawWireSphere(gameObject.transform.position, maxDetectionDistance);

        Gizmos.DrawWireCube(new Vector3(transform.position.x - 0.5f, transform.position.y + 0.65f, 0f), new Vector3(0.15f, 1.15f, 0f));
        Gizmos.DrawWireCube(new Vector3(transform.position.x + 0.5f, transform.position.y + 0.65f, 0f), new Vector3(0.15f, 1.15f, 0f));

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
#endif
}
