using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class bora : MonoBehaviour
{
    [Header("Basic Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 10f;

    [Header("Wall Jump")]
    public float wallJumpForce = 10f;
    public float wallSlideSpeed = 2f;
    public float wallJumpDirectionForce = 8f;
    public float momentumDuration = 0.2f;
    public float momentumControlMultiplier = 0.3f;
    public float wallJumpBoostSpeed = 10f;
    public float wallKeyDisableTime = 0.5f;
    public LayerMask wallLayer;
    public bool enableUpwardWallJumpBug = false;  // Toggle for the "feature"

    [Header("Air Control")]
    public float airControlForce = 40f;
    public float airMaxSpeed = 8f;

    [Header("Fast Fall")]
    public float fastFallMultiplier = 2f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public LayerMask enemyLayer;

    [Header("UI")]
    public TextMeshProUGUI velocityDisplay;

    [Header("Events")]
    public UnityEvent OnDash;
    public UnityEvent OnJump;
    public UnityEvent OnLand;
    public UnityEvent OnWallJump;

    [Header("Momentum Settings")]
    public float momentumPreserveMultiplier = 1f; // 1 = full momentum, 0.5 = half momentum, etc.

    [Header("Bunny Hop")]
    public float bhopMultiplier = 1f;  // Control momentum preservation
    private float lastVelocityBeforeLanding;  // Store the velocity
    private float preservedSpeed;  // New variable to store the speed we want to preserve

    private Rigidbody2D rb;
    private bool canJump = true;
    private bool isWallSliding = false;
    private bool canWallJump = false;
    private bool isDashing;

    private float dashTimeLeft;
    private float dashCooldownTimer;
    private int facingDirection = 1;
    private Vector2 dashDirection;
    private int originalLayer;

    private bool preservingMomentum;
    private float momentumTimeLeft;
    private float wallKeyDisableTimeLeft;
    private KeyCode disabledKey;

    private bool isWallLeft;
    private bool isWallRight;

    public int FacingDirection => facingDirection;
    public bool CanJump => canJump;
    public Vector2 Velocity => rb.linearVelocity;
    public bool IsGrounded => canJump;
    public bool IsWallSliding => isWallSliding;
    public bool IsWallLeft => isWallLeft;
    public bool IsWallRight => isWallRight;
    public bool IsDashing => isDashing;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalLayer = gameObject.layer;
    }

    void Update()
    {
        UpdateVelocityDisplay();

        if (isDashing)
        {
            HandleDash();
            return;
        }

        if (Input.GetKey(KeyCode.LeftControl) && dashCooldownTimer <= 0)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            if (horizontal != 0 || vertical != 0)
            {
                dashDirection = new Vector2(horizontal, vertical).normalized;
            }
            else
            {
                dashDirection = new Vector2(facingDirection, 0);
            }
            
            StartDash();
            return;
        }

        HandleMovement();

        if (Input.GetKey(KeyCode.S) && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.down * fastFallMultiplier * Time.deltaTime;
        }

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        // Store velocity when in air
        if (!canJump)
        {
            lastVelocityBeforeLanding = rb.linearVelocity.x;
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");

        if (moveX != 0)
        {
            facingDirection = (int)Mathf.Sign(moveX);
        }

        if (wallKeyDisableTimeLeft > 0)
        {
            wallKeyDisableTimeLeft -= Time.deltaTime;
            if ((disabledKey == KeyCode.A && moveX < 0) || (disabledKey == KeyCode.D && moveX > 0))
            {
                moveX = 0;
            }
        }

        if (preservingMomentum)
        {
            momentumTimeLeft -= Time.deltaTime;
            if (momentumTimeLeft <= 0)
            {
                preservingMomentum = false;
            }
            else
            {
                float controlForce = moveX * airControlForce * momentumControlMultiplier;
                rb.AddForce(new Vector2(controlForce, 0) * Time.deltaTime, ForceMode2D.Impulse);

                float targetSpeed;
                if (facingDirection > 0)
                {
                    targetSpeed = (moveX < 0) ? wallJumpBoostSpeed : wallJumpDirectionForce;
                }
                else
                {
                    targetSpeed = (moveX > 0) ? wallJumpBoostSpeed : wallJumpDirectionForce;
                }

                float minSpeed = targetSpeed * (momentumTimeLeft / momentumDuration);
                float currentXVel = Mathf.Abs(rb.linearVelocity.x);
                
                if (currentXVel < minSpeed)
                {
                    rb.linearVelocity = new Vector2(-facingDirection * minSpeed, rb.linearVelocity.y);
                }
            }
        }

        if (canJump)
        {
            rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            float airForce = moveX * airControlForce;
            rb.AddForce(new Vector2(airForce, 0) * Time.deltaTime, ForceMode2D.Impulse);
            rb.linearVelocity = new Vector2(
                Mathf.Clamp(rb.linearVelocity.x, -airMaxSpeed, airMaxSpeed),
                rb.linearVelocity.y);
        }

        HandleWallJump(moveX);

        if (Input.GetKey(KeyCode.Space) && canJump)
        {
            // For the first jump, maintain horizontal velocity
            if (preservedSpeed == 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else
            {
                rb.linearVelocity = new Vector2(preservedSpeed, jumpForce);
            }
            
            canJump = false;
            OnJump?.Invoke();
        }
    }

    void HandleWallJump(float moveX)
    {
        if (isWallSliding)
        {
            bool holdingTowardsWall = (isWallRight && moveX > 0) || (isWallLeft && moveX < 0);

            if (holdingTowardsWall)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
            }

            if (Input.GetKey(KeyCode.Space) && canWallJump)
            {
                // Check if we should use the "bug" behavior
                bool facingAwayFromWall = (isWallRight && facingDirection < 0) || (isWallLeft && facingDirection > 0);
                
                if (enableUpwardWallJumpBug && facingAwayFromWall)
                {
                    // Jump straight up instead of away from wall
                    rb.linearVelocity = new Vector2(0, wallJumpForce * 1.2f);  // Slightly higher jump for fun
                }
                else
                {
                    // Normal wall jump away from wall
                    float jumpDirectionX = isWallRight ? -1 : 1;
                    rb.linearVelocity = new Vector2(jumpDirectionX * wallJumpDirectionForce, wallJumpForce);
                }
                
                preservingMomentum = true;
                momentumTimeLeft = momentumDuration;
                
                wallKeyDisableTimeLeft = wallKeyDisableTime;
                disabledKey = isWallRight ? KeyCode.D : KeyCode.A;
                
                isWallSliding = false;
                canWallJump = false;
                OnWallJump?.Invoke();
            }
        }
    }

    public void ResetDash()
    {
        dashCooldownTimer = 0f;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownTimer = dashCooldown;
        rb.gravityScale = 0;
        
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);
        
        OnDash?.Invoke();
    }

    void HandleDash()
    {
        if (dashTimeLeft > 0)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            dashTimeLeft -= Time.deltaTime;

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 0.5f, enemyLayer);
            foreach (Collider2D enemy in hitEnemies)
            {
                ResetDash();
                break;
            }
        }
        else
        {
            EndDash();
        }
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 1;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);
    }

    void UpdateVelocityDisplay()
    {
        if (velocityDisplay)
        {
            velocityDisplay.text = $"Velocity: X = {rb.linearVelocity.x:F2}, Y = {rb.linearVelocity.y:F2}";
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.GetContact(0).normal.y > 0.5f)
        {
            bool wasGrounded = canJump;
            canJump = true;
            isWallSliding = false;
            canWallJump = false;

            isWallLeft = false;
            isWallRight = false;

            // Reset velocity if not holding jump
            if (!Input.GetKey(KeyCode.Space))
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                preservedSpeed = 0;  // Make sure preserved speed is reset
            }
            else
            {
                preservedSpeed = lastVelocityBeforeLanding * bhopMultiplier;
                Debug.Log($"Bhop Speed Stored: {preservedSpeed}");
            }

            if (!wasGrounded)
            {
                OnLand?.Invoke();
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        canJump = false;
        isWallSliding = false;
        isWallLeft = false;
        isWallRight = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.GetContact(0).normal.y > 0.5f)
        {
            canJump = true;
            isWallSliding = false;
            canWallJump = false;

            isWallLeft = false;
            isWallRight = false;

            return;
        }

        // Only check wall collision if we're in the air and it's a wall
        if (!canJump && ((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            Vector2 normal = collision.GetContact(0).normal;
            if (Mathf.Abs(normal.x) > 0.5f)
            {
                isWallSliding = true;
                canWallJump = true;

                isWallLeft = normal.x > 0;
                isWallRight = normal.x < 0;
            }
        }
    }
}