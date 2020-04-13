using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Mirror;

[RequireComponent(typeof(PlayerController))]
public class Player : NetworkBehaviour
{
    [Header("Player Components")]
    public PlayerController controller;
    public Camera playerCamera;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D playerBody2D;
    public Health healthBar;
    public Transform spawnPoint;

    [Header("Health & Damage")]
    [SerializeField] private float health = 10f;
    [SyncVar] private float currentHealth;
    public float damage = 2f;
    [SerializeField] private Transform attackPointRight = null;
    [SerializeField] private Transform attackPointLeft = null;
    [SerializeField] private float attackRange = 1f;

    [Header("Jump Settings")]
    public float maxJumpHeight = 10f;
    public float minJumpHeight = 2f;
    public float timeToJumpApex = 0.4f;

    [Header("Movement Speed")]
    public float speed = 6f;

    [Header("Ability Settings")]
    public float dashForce = 10f;

    [Tooltip("Dash cooldown in seconds")]
    public float dashCooldown = 10f;

    private const float maxGravity = -20f;

    [Header("Acceleration smoothing")]
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;
    private float velocityXSmoothing;

    private float maxJumpVelocity;
    private float minJumpVelocity;
    private float gravity;

    private Vector2 directionalInput;
    private Vector2 velocity;

    private IEnumerator dashCorotine;
    private bool canDash = true;
    private bool isOnGround = false;
    private bool hurtCd = false;

    public float GetHP => currentHealth;
    public Vector2 GetDirInput => directionalInput;
    private Vector2 lastDirInput;

    private const float maxJumpMultiplier = 0.6f;
    private const float minJumpMultiplier = 0.035f;

    // Instead of color, "Player 1, Player 2" or etc.
    private List<Color> colorList = new List<Color>
    {
        Color.green,
        Color.red,
        Color.blue,
        Color.white,
        Color.magenta,
        Color.cyan,
        Color.grey,
        Color.yellow,
    };

    private enum animState
    {
        idle,
        combat,
        run,
    }

    private void Awake()
    {
        SetupComponents();
    }

    private void SetupComponents()
    {
        if (spawnPoint)
        {
            transform.position = spawnPoint.position;
        }
        else
        {
            transform.position = new Vector3(0, 10, 0);
        }

        if (!controller)
        {
            controller = GetComponent<PlayerController>();
        }

        if (!playerCamera)
        {
            playerCamera = GetComponent<Camera>();
        }

        if (!animator)
        {
            animator = GetComponent<Animator>();
        }

        if (!spriteRenderer)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (!playerBody2D)
        {
            playerBody2D = GetComponent<Rigidbody2D>();
        }

        if (!isLocalPlayer)
        {
            spriteRenderer.color = colorList[Random.Range(0, colorList.Count)];
        }
    }

    private void Start()
    {
        currentHealth = health;
        healthBar.SetMaxHealth(health);

        gravity = -((2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2));
        maxJumpVelocity = ((Mathf.Abs(gravity) * timeToJumpApex) * maxJumpMultiplier);
        minJumpVelocity = ((Mathf.Abs(gravity) * minJumpHeight) * minJumpMultiplier);
    }

    private void Update()
    {
        UpdateMovement();

        FlipSpriteX();

        IdleAnimations();
    }

    private void UpdateMovement()
    {
        controller.boxController.UpdateCollision(velocity * Time.deltaTime, directionalInput);
        CalculateVelocity();
        controller.Move(velocity * Time.deltaTime);
    }

    private void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * speed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
            (controller.boxController.collisionsInfo.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        bool hitCeiling = controller.boxController.collisionsInfo.above ? true : false;

        if (hitCeiling)
        {
            velocity.y = velocity.y > Mathf.Abs(gravity * 0.3f) ? gravity * 0.3f : -velocity.y;
        }

        if (!controller.boxController.collisionsInfo.below)
        {
            // Attack in air
            if (!AnimationIsPlaying(StringData.attack) && AnimationIsPlaying(StringData.jump))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    animator.StopPlayback();
                    animator.SetTrigger(StringData.attack);
                }
            }

            velocity.y += gravity * Time.deltaTime;

            if (velocity.y < maxGravity)
            {
                velocity.y = maxGravity;
            }

            isOnGround = false;
        }
        else
        {
            if (velocity.y <= 0)
            {
                if (AnimationIsPlaying(StringData.jump))
                {
                    animator.StopPlayback();
                }

                animator.SetBool(StringData.grounded, true);
                velocity.y = 0;
                isOnGround = true;
            }
        }

        // Stop player from shaking when moving towards a wall
        if (controller.boxController.collisionsInfo.right)
        {
            velocity.x = Mathf.Clamp(velocity.x, float.NegativeInfinity, 0);
        }
        if (controller.boxController.collisionsInfo.left)
        {
            velocity.x = Mathf.Clamp(velocity.x, 0, float.PositiveInfinity);
        }
    }

    private void FlipSpriteX()
    {
        if (Mathf.Abs(directionalInput.x) > Mathf.Epsilon)
        {
            animator.SetInteger(StringData.animState, (int)animState.run);
            spriteRenderer.flipX = directionalInput.x > 0f;
        }
    }

    private void SetDirectionalInput(Vector2 directionalInput)
    {
        if (directionalInput.x != 0f)
        {
            lastDirInput = this.directionalInput;
        }

        this.directionalInput = directionalInput;
    }

    private void OnJumpInputDown()
    {
        if (isOnGround)
        {
            velocity.y = maxJumpVelocity;

            JumpAnimation();
        }
    }

    private void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }

    private void OnAttackInputDown()
    {
        var point = lastDirInput.x > 0f ? attackPointRight.position : attackPointLeft.position;

        if (!AnimationIsPlaying(StringData.attack))
        {
            animator.SetTrigger(StringData.attack);

            Collider2D[] hitByAttack = Physics2D.OverlapCircleAll(point, attackRange, LayerMask.GetMask(StringData.enemyLayer));

            foreach (var col in hitByAttack)
            {
                col.GetComponent<AI>()?.TakeDamage(damage);
            }
        }
    }

    private void DashCooldown()
    {
        dashCorotine = DashCooldownTimer(dashCooldown);
        StartCoroutine(dashCorotine);
    }

    private IEnumerator DashCooldownTimer(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);
        canDash = true;
    }

    public void OnEscapeInputDown()
    {
        Debug.Log("GameMenu?");
    }

    private bool AnimationIsPlaying(string state)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(state);
    }

    // Falling / Jump
    private void JumpAnimation()
    {
        animator.SetTrigger(StringData.jump);
        animator.SetBool(StringData.grounded, false);
    }

    private void OnShiftInputDown()
    {
        if (canDash)
        {
            playerBody2D.AddForce(directionalInput.normalized * dashForce, ForceMode2D.Impulse);
            canDash = false;
            DashCooldown();
        }
    }

    private void IdleAnimations()
    {
        if (Mathf.Abs(directionalInput.x) < Mathf.Epsilon)
        {
            animator.SetInteger(StringData.animState, (int)animState.idle);
        }

        if (controller.boxController.collisionsInfo.below || isOnGround)
        {
            animator.SetBool(StringData.grounded, true);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        
        if (currentHealth <= 0f)
        {
            StartCoroutine(Die(1f));
            return;
        }
        
        if (currentHealth > 1f && !hurtCd)
        {
            StartCoroutine(HurtAnimation(0.5f));
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

            GetComponent<BoxCollider2D>().isTrigger = true;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

            NetworkServer.UnSpawn(gameObject);
        }
    }

    [Command]
    public void CmdOnShiftInputDown()
    {
        RpcOnShiftInputDown();
    }

    [Command]
    public void CmdOnAttackInputDown()
    {
        RpcOnAttackInputDown();
    }

    [Command]
    public void CmdSetDirectionalInput(Vector2 directionalInput)
    {
        RpcSetDirectionalInput(directionalInput);
    }

    [Command]
    public void CmdOnJumpInputDown()
    {
        RpcOnJumpInputDown();
    }

    [Command]
    public void CmdOnJumpInputUp()
    {
        RpcOnJumpinputUp();
    }

    [ClientRpc]
    private void RpcOnShiftInputDown()
    {
        OnShiftInputDown();
    }

    [ClientRpc]
    private void RpcOnAttackInputDown()
    {
        OnAttackInputDown();
    }

    [ClientRpc]
    private void RpcOnJumpinputUp()
    {
        OnJumpInputUp();
    }

    [ClientRpc]
    private void RpcOnJumpInputDown()
    {
        OnJumpInputDown();
    }

    [ClientRpc]
    private void RpcSetDirectionalInput(Vector2 directionalInput)
    {
        SetDirectionalInput(directionalInput);
    }
}


