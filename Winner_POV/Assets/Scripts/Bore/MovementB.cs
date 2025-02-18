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
    public bool enableUpwardWallJumpBug = false;

    [Header("Air Control")]
    public float airControlForce = 40f;
    public float airMaxSpeed = 8f;
    private bool bypassSpeedLimit = false;
    private float temporarySpeedCap = 8f;

    [Header("Fast Fall")]
    public float fastFallMultiplier = 2f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public LayerMask enemyLayer;
    private Vector2 velocityBeforeDash;

    [Header("UI")]
    public TextMeshProUGUI velocityDisplay;

    [Header("Events")]
    public UnityEvent OnDash;
    public UnityEvent OnJump;
    public UnityEvent OnLand;
    public UnityEvent OnWallJump;

    [Header("Momentum Settings")]
    public float momentumPreserveMultiplier = 1f;

    [Header("Bunny Hop")]
    public float bhopMultiplier = 1f;
    private float lastVelocityBeforeLanding;
    private float preservedSpeed;

    [Header("Wall Slide Settings")]
    public GameObject objectToDisable; // Reference to the GameObject to disable

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

    public void SetSpeedLimitBypass(bool bypass, float speedCap)
    {
        bypassSpeedLimit = bypass;
        temporarySpeedCap = speedCap;
    }

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
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = ((Vector2)(mousePos - transform.position)).normalized;
            
            dashDirection = direction;
            velocityBeforeDash = rb.linearVelocity;
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
            float airForce = moveX * airControlForce * Time.deltaTime;
            float currentXVel = rb.linearVelocity.x;

            if ((moveX > 0 && currentXVel < airMaxSpeed) || 
                (moveX < 0 && currentXVel > -airMaxSpeed))
            {
                float projectedVelocity = currentXVel + airForce;
                
                if (Mathf.Abs(projectedVelocity) > airMaxSpeed)
                {
                    float clampedForce = (airMaxSpeed * Mathf.Sign(moveX)) - currentXVel;
                    rb.AddForce(new Vector2(clampedForce, 0), ForceMode2D.Impulse);
                }
                else
                {
                    rb.AddForce(new Vector2(airForce, 0), ForceMode2D.Impulse);
                }
            }

            if (bypassSpeedLimit)
            {
                rb.linearVelocity = new Vector2(
                    Mathf.Clamp(rb.linearVelocity.x, -temporarySpeedCap, temporarySpeedCap),
                    rb.linearVelocity.y
                );
            }
        }

        HandleWallJump(moveX);

        if (Input.GetKey(KeyCode.Space) && canJump)
        {
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
            // Disable the GameObject when wall sliding
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }

            bool holdingTowardsWall = (isWallRight && moveX > 0) || (isWallLeft && moveX < 0);

            if (holdingTowardsWall)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
            }

            if (Input.GetKey(KeyCode.Space) && canWallJump)
            {
                bool facingAwayFromWall = (isWallRight && facingDirection < 0) || (isWallLeft && facingDirection > 0);
                
                if (enableUpwardWallJumpBug && facingAwayFromWall)
                {
                    rb.linearVelocity = new Vector2(0, wallJumpForce * 1.2f);
                }
                else
                {
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
        else
        {
            // Enable the GameObject when not wall sliding
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(true);
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

        if (!canJump)
        {
            float blendFactor = 0.3f;
            Vector2 dashVelocity = dashDirection * dashSpeed;
            Vector2 blendedVelocity = Vector2.Lerp(velocityBeforeDash, dashVelocity, blendFactor);
            
            blendedVelocity.x = Mathf.Clamp(blendedVelocity.x, -airMaxSpeed, airMaxSpeed);
            
            rb.linearVelocity = new Vector2(blendedVelocity.x, velocityBeforeDash.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        
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

            if (!Input.GetKey(KeyCode.Space))
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                preservedSpeed = 0;
            }
            else
            {
                preservedSpeed = lastVelocityBeforeLanding * bhopMultiplier;
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