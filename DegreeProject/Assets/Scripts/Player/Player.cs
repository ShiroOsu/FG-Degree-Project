using UnityEngine;
using System.Collections.Generic;
using Mirror;

[RequireComponent(typeof(PlayerController))]
public class Player : NetworkBehaviour
{
    [Header("Player Settings")]
    public PlayerController controller;
    public Camera playerCamera;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D playerBody2D;
    public Transform spawnPoint;

    [Header("Player's health & Damage")]
    [SyncVar] public float health = 10f;
    public float damage = 2f;

    [Header("Jump Settings")]
    public float maxJumpHeight = 6f;
    public float minJumpHeight = 2f;
    public float timeToJumpApex = 0.4f;

    [Header("Movement Speed")]
    public float speed = 6f;

    [Header("Ability Settings")]
    public float dashForce = 10f;

    [Tooltip("Dash cooldown in seconds")]
    public float dashCooldown = 10f;

    [Header("Gravity"), Tooltip("Should be a negative value")]
    public float maxGravity = -20f;

    [Header("Acceleration smoothing (Airborne and Ground)")]
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;
    private float velocityXSmoothing;

    private float maxJumpVelocity;
    private float minJumpVelocity;
    private float gravity;

    private float timeStamp;

    [SyncVar] private Vector2 directionalInput;
    [SyncVar] private Vector2 velocity;

    private bool canDash = true;
    private bool isDead = false;

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

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        if (spawnPoint)
        {
            transform.position = spawnPoint.position;
        }
        else
        {
            transform.position = new Vector3(0, 10, 0);
        }

        // If gravity happened to be a positive value, invert it.
        if (gravity > 0)
        {
            gravity *= -1;
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

        gravity = -((2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2));
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        if (!isLocalPlayer)
        {
            spriteRenderer.color = colorList[Random.Range(0, colorList.Count)];
        }
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            CalculateVelocity();
        }
    }

    private void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * speed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
            (controller.collisionInfo.under) ? accelerationTimeGrounded : accelerationTimeAirborne);

        bool hittedCeiling = controller.collisionInfo.above ? true : false;

        if (hittedCeiling)
        {
            playerBody2D.AddForce(Vector2.down, ForceMode2D.Impulse);
        }

        if (!controller.collisionInfo.under)
        {
            JumpAnimation();

            velocity.y += gravity * Time.deltaTime;

            if (velocity.y < maxGravity)
            {
                velocity.y = maxGravity;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            controller.Move(velocity * Time.fixedDeltaTime);

            CmdDashCooldown();
        }

        FlipSpriteX();

        IdleAnimations();
    }

    private void FlipSpriteX()
    {
        if (Mathf.Abs(directionalInput.x) > Mathf.Epsilon)
        {
            animator.SetInteger(StringData.animState, 2); // Run animation

            if (directionalInput.x > Mathf.Epsilon)
            {
                spriteRenderer.flipX = true;
            }
            else if (directionalInput.x < Mathf.Epsilon)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    private void SetDirectionalInput(Vector2 directionalInput)
    {
        this.directionalInput = directionalInput;
    }

    private void OnJumpInputDown()
    {
        if (controller.collisionInfo.under)
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
        if (!AnimationIsPlaying(StringData.attack))
        {
            animator.SetTrigger(StringData.attack);
        }
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
        animator.SetTrigger(StringData.jump); // Jump animation
        animator.SetBool(StringData.grounded, false); // To continue being in jump animation set grounded to false
    }

    private void OnShiftInputDown()
    {
        if (canDash)
        {
            playerBody2D.AddForce(directionalInput.normalized * dashForce, ForceMode2D.Impulse);
            canDash = false;
            timeStamp = Time.time + dashCooldown;
        }
    }

    private void DashCooldown()
    {
        if (!canDash)
        {
            if ((timeStamp - Time.time) > -1f)
            {
                if (timeStamp <= Time.time)
                {
                    canDash = true;
                }
            }
        }
    }

    private void IdleAnimations()
    {
        if (Mathf.Abs(directionalInput.x) < Mathf.Epsilon)
        {
            animator.SetInteger(StringData.animState, 0); // Idle animation
        }

        if (controller.collisionInfo.under)
        {
            animator.SetBool(StringData.grounded, true); // Standing animation (Idle)
        }
    }

    private void TakeDamage(float damage)
    {
        health -= damage;

        // Damage animation
        animator.SetTrigger(StringData.hurt);

        if (health <= 0f)
        {
            isDead = true;
        }
    }

    private void ShouldDie()
    {
        if (isDead)
        {
            if (gameObject != null)
            {
                // Death animation
                animator.SetTrigger(StringData.death);

                // We want to be able to re-spawn the player after it has died,
                // see we only unspawn it from the server.
                NetworkServer.UnSpawn(gameObject);
                
                //if (base.hasAuthority)
                //{
                //    //CmdRespawn();
                //}

                // Spectate alive player; ?
            }
        }
    }

    #region Command Functions

    [Command]
    public void CmdDashCooldown()
    {
        DashCooldown();
        RpcDashCooldown();
    }

    [Command]
    public void CmdOnShiftInputDown()
    {
        OnShiftInputDown();
        RpcOnShiftInputDown();
    }

    [Command]
    public void CmdOnAttackInputDown()
    {
        OnAttackInputDown();
        RpcOnAttackInputDown();
    }

    [Command]
    public void CmdSetDirectionalInput(Vector2 directionalInput)
    {
        SetDirectionalInput(directionalInput);
        RpcSetDirectionalInput(directionalInput);
    }

    [Command]
    public void CmdOnJumpInputDown()
    {
        OnJumpInputDown();
        RpcOnJumpInputDown();
    }

    // Seems to work without Command/Rpc
    [Command]
    public void CmdOnJumpInputUp()
    {
        OnJumpInputUp();
        RpcOnJumpinputUp();
    }

    #endregion Command Functions

    #region ClientRpc Functions

    [ClientRpc]
    private void RpcDashCooldown()
    {
        DashCooldown();
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

    #endregion ClientRpc Functions

}


