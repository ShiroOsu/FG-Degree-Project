using UnityEngine;
using System.Collections.Generic;
using Mirror;

[RequireComponent(typeof(PlayerController))]
public class Player : NetworkBehaviour
{
    [Header("Player Controller")]
    public PlayerController controller;

    [Header("Player Camera")]
    public Camera playerCamera;

    [Header("Player Animator")]
    public Animator animator;

    [Header("Player Sprite Renderer")]
    public SpriteRenderer spriteRenderer;

    [Header("Player Rigidbody2D"),]
    public Rigidbody2D playerBody2D;

    [Header("Player's health & Damage")]
    [SyncVar] public float health = 10f;
    public float damage = 2f;

    [Header("Jump Settings")]
    public float maxJumpHeight = 6f;
    public float minJumpHeight = 2f;
    public float timeToJumpApex = 0.4f;

    [Header("Movement Speed")]
    public float speed = 6f;

    [Header("Dash Force")] 
    public float dashForce = 10f;
    
    [Tooltip("Dash cooldown in seconds")]
    public float dashCooldown = 10f;

    [Header("Gravity"), Tooltip("Should be a negative value")]
    public float maxGravity = -20f;

    [Header("Acceleration smoothing (Airborne and Ground)")]
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;
    private float velocityXSmoothing;

    [Header("Spawn point for player")]
    public Transform spawnPoint;

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
        CalculateVelocity();
    }

    public void CalculateVelocity()
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
        controller.Move(velocity * Time.fixedDeltaTime);

        FlipSpriteX();

        IdleAnimations();

        if (isLocalPlayer)
        {
            CmdDashCooldown();
        }
    }

    public void FlipSpriteX()
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

    public void SetDirectionalInput(Vector2 directionalInput)
    {
        this.directionalInput = directionalInput;
    }

    public void OnJumpInputDown()
    {
        if (controller.collisionInfo.under)
        {
            velocity.y = maxJumpVelocity;

            JumpAnimation();
        }
    }

    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }

    public void OnAttackInputDown()
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

    public bool AnimationIsPlaying(string state)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(state);
    }

    // Falling / Jump
    public void JumpAnimation()
    {
        animator.SetTrigger(StringData.jump); // Jump animation
        animator.SetBool(StringData.grounded, false); // To continue being in jump animation set grounded to false
    }

    public void OnShiftInputDown()
    {
        if (canDash)
        {
            playerBody2D.AddForce(directionalInput.normalized * dashForce, ForceMode2D.Impulse);
            canDash = false;
            timeStamp = Time.time + dashCooldown;
        }
    }

    public void DashCooldown()
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

    public void IdleAnimations()
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

    public void TakeDamage(float damage)
    {
        health -= damage;

        // Damage animation
        animator.SetTrigger(StringData.hurt);

        if (health <= 0f)
        {
            isDead = true;
        }
    }

    public void ShouldDie()
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
    public void RpcDashCooldown()
    {
        DashCooldown();
    }

    [ClientRpc]
    public void RpcOnShiftInputDown()
    {
        OnShiftInputDown();
    }

    [ClientRpc]
    public void RpcOnAttackInputDown()
    {
        OnAttackInputDown();
    }

    [ClientRpc]
    public void RpcOnJumpinputUp()
    {
        OnJumpInputUp();
    }

    [ClientRpc]
    public void RpcOnJumpInputDown()
    {
        OnJumpInputDown();
    }

    [ClientRpc]
    public void RpcSetDirectionalInput(Vector2 directionalInput)
    {
        SetDirectionalInput(directionalInput);
    }

    #endregion ClientRpc Functions

}


