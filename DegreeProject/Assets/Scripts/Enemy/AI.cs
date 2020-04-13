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
    public Transform attackPointRight;
    public Transform attackPointLeft;
    public float attackRange = 1f;
    public float attackRate = 0.25f;

    public Vector2 dir { get; private set; }
    private bool hurtCd = false;

    [Header("Detection Settings")]
    [SerializeField] private float maxDetectionDistance = 8f;
    public float detectionRadius = 5f;
    public float sqrCurrDistance { get; set; }
    public float sqrMaxDistance => maxDetectionDistance * maxDetectionDistance;

    [Header("AI Components")]
    public Health healthBar;

    // AI components
    public Collider2D[] detectionCircle { get; set; }
    public GameObject playerObject { get; set; }
    public StateMachine<AI> stateMachine { get; private set; }
    public FollowState followState { get; private set; }
    public PatrolState patrolState { get; private set; }
    public AttackState attackState { get; private set; }
    public Animator animator { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }

    public enum AnimState
    {
        idle,
        combat,
        run,
    }

    private void Start()
    {
        followState = new FollowState();
        patrolState = new PatrolState();
        attackState = new AttackState();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        stateMachine = new StateMachine<AI>(this);
        stateMachine.ChangeState(patrolState);

        currentHealth = health;
        healthBar.SetMaxHealth(health);
    }

    private void Update()
    {
        UpdateDistanceToPlayer();

        stateMachine.UpdateState();

        FlipSpriteX();
    }

    private void UpdateDistanceToPlayer()
    {
        if (playerObject != null)
        {
            sqrCurrDistance = (playerObject.transform.position - transform.position).sqrMagnitude;
        }
    }

    public void PatrolMove(Vector2 direction)
    {
        transform.Translate(direction * speed * Time.deltaTime);

        RpcAnimState(AnimState.run);
    }

    public void FollowMove()
    {
        // Move towards playerObjects location (x axis)
        transform.position = Vector2.MoveTowards(transform.position,
            new Vector2(playerObject.transform.position.x, transform.position.y), speed * Time.deltaTime);

        RpcAnimState(AnimState.run);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        
        if (currentHealth <= 0f)
        {
            RpcDieAnimation();
            return;
        }

        if (currentHealth > 0f & !hurtCd)
        {
            RpcHurtAnimation();
            hurtCd = true;
            StartCoroutine(HurtAnimCd(1.5f));
        }
    }

    private IEnumerator HurtAnimCd(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        hurtCd = false;
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
        RpcAttackAnimation();
    }

    public void SetDirection(Vector2 direction)
    {
        dir = direction;
    }

    private void FlipSpriteX()
    {
        if (playerObject != null)
        {
            var playerDir = playerObject.GetComponent<Player>().GetDirInput;

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
            spriteRenderer.flipX = dir.x > 0f;
        }
    }

    [ClientRpc]
    private void RpcHurtAnimation()
    {
        StartCoroutine(HurtAnimation(0.5f));
    }

    [ClientRpc]
    private void RpcDieAnimation()
    {
        StartCoroutine(Die(1f));
    }

    [ClientRpc]
    public void RpcAttackAnimation()
    {
        animator.SetTrigger(StringData.attack);
    }

    [ClientRpc]
    public void RpcAnimState(AnimState animation)
    {
        animator.SetInteger(StringData.animState, (int)animation);
    }

#if DEBUG
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(gameObject.transform.position, detectionRadius);
        Gizmos.DrawWireSphere(gameObject.transform.position, maxDetectionDistance);

        Gizmos.DrawWireCube(new Vector3(transform.position.x - 0.5f, transform.position.y + 0.65f, 0f), new Vector3(0.15f, 1.15f, 0f));
        Gizmos.DrawWireCube(new Vector3(transform.position.x + 0.5f, transform.position.y + 0.65f, 0f), new Vector3(0.15f, 1.15f, 0f));
    }
#endif
}
