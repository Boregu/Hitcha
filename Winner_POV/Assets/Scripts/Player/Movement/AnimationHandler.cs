using UnityEngine;
using System.Collections;

public class AnimationHandler : MonoBehaviour
{
    [Header("Components")]
    public float LastNonZeroInput => lastNonZeroInput;
    public Animator parentAnimator;    // Legs/Body animator
    public Animator upperBodyAnimator; // Upper body animator
    public SpriteRenderer spriteRenderer;
    private bora movement;
    private PlayerAttack playerAttack;
    private float lastNonZeroInput;

    [Header("Animation States")]
    public bool IsPunching => upperBodyAnimator.GetBool("IsPunch");
    public bool IsHeavyPunching => upperBodyAnimator.GetBool("IsHeavyPunch");
    public bool IsShooting => upperBodyAnimator.GetBool("IsShoot");
    public bool IsUppercutting => upperBodyAnimator.GetBool("IsUppercut");
    public bool IsIdle => !IsPunching && !IsHeavyPunching && !IsShooting && !IsUppercutting;

    [Header("Animation Timing")]
    public float heavyPunchAnimDuration = 0.5f;
    public float uppercutAnimDuration = 0.4f;
    [Tooltip("Velocity threshold for when to reset jump animation before landing (-2 means reset when falling slower than 2 units/sec)")]
    public float visualGroundedThreshold = -2f;

    private bool wasGrounded;
    private bool wasVisuallyGrounded;
    private Rigidbody2D rb;

    void Start()
    {
        movement = GetComponent<bora>();
        playerAttack = GetComponent<PlayerAttack>();
        rb = GetComponent<Rigidbody2D>();

        if (parentAnimator == null)
            parentAnimator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (upperBodyAnimator == null)
            Debug.LogError("Upper body animator not assigned!");
    }

    void Update()
    {
        HandleMovementAnimations();
        HandleUpperBodyAnimations();
    }

    void HandleMovementAnimations()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        bool isGrounded = movement.IsGrounded;
        bool isDashing = movement.IsDashing;
        bool isWallSliding = movement.IsWallSliding;
        
        // Check for visual grounded state (when falling and close to ground)
        bool isVisuallyGrounded = isGrounded || (!isGrounded && rb.linearVelocity.y > visualGroundedThreshold && rb.linearVelocity.y < 0);
        
        // Reset jump animation when entering visual grounded state
        if (!wasVisuallyGrounded && isVisuallyGrounded)
        {
            parentAnimator.SetBool("IsJump", false);
        }
        wasVisuallyGrounded = isVisuallyGrounded;

        // Update facing direction based on A/D keys
        if (moveX != 0)
        {
            lastNonZeroInput = moveX;
        }

        // Set basic movement parameters
        parentAnimator.SetBool("IsIdle", moveX == 0 && isGrounded);
        parentAnimator.SetBool("IsRun", Mathf.Abs(moveX) > 0 && isGrounded);
        parentAnimator.SetBool("IsDash", isDashing);
        
        // Handle air states (jumping and wall sliding) together
        if (isWallSliding)
        {
            parentAnimator.SetBool("IsJump", false);
            parentAnimator.SetBool("IsWallSlideLeft", movement.IsWallLeft);
            parentAnimator.SetBool("IsWallSlideRight", movement.IsWallRight);
        }
        else if (!isVisuallyGrounded) // Use visual grounded check for jump animation
        {
            parentAnimator.SetBool("IsWallSlideLeft", false);
            parentAnimator.SetBool("IsWallSlideRight", false);
            parentAnimator.SetBool("IsJump", true);
        }

        // Set facing direction based on last pressed A/D key
        bool isFacingLeft = lastNonZeroInput < 0;
        parentAnimator.SetBool("IsFacingLeft", isFacingLeft);
        parentAnimator.SetBool("IsFacingRight", !isFacingLeft);

        // Just flip the sprite renderer
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isFacingLeft;
        }
    }

    void HandleUpperBodyAnimations()
    {
        // Set default idle state if no other animations are playing
        if (IsIdle)
        {
            upperBodyAnimator.SetBool("IsIdle", true);
        }
        else
        {
            upperBodyAnimator.SetBool("IsIdle", false);
        }

        // Heavy Punch Animation States
        if (playerAttack.IsChargingHeavyPunch)
        {
            upperBodyAnimator.SetBool("IsChargingHeavyPunch", true);
            upperBodyAnimator.SetBool("IsHeavyPunch", false);
        }
        else
        {
            upperBodyAnimator.SetBool("IsChargingHeavyPunch", false);
        }

        if (playerAttack.IsHeavyPunchFullyCharged)
        {
            upperBodyAnimator.SetBool("IsHeavyPunchCharged", true);
        }
        else
        {
            upperBodyAnimator.SetBool("IsHeavyPunchCharged", false);
        }

        // Uppercut Animation
        if (playerAttack.IsUppercutting)
        {
            upperBodyAnimator.SetBool("IsUppercut", true);
            upperBodyAnimator.SetBool("IsIdle", false);
            StartCoroutine(ResetAnimationAfterDelay("IsUppercut", uppercutAnimDuration));
        }

        // Normal Attack Animation
        if (Input.GetMouseButtonDown(1))
        {
            upperBodyAnimator.SetBool("IsPunch", true);
            upperBodyAnimator.SetBool("IsIdle", false);
            StartCoroutine(ResetAnimationAfterDelay("IsPunch", 0.2f));
        }

        // Shooting Animation
        if (Input.GetMouseButtonDown(0))
        {
            upperBodyAnimator.SetBool("IsShoot", true);
            upperBodyAnimator.SetBool("IsIdle", false);
            StartCoroutine(ResetAnimationAfterDelay("IsShoot", 0.2f));
        }
    }

    System.Collections.IEnumerator ResetAnimationAfterDelay(string paramName, float delay)
    {
        yield return new WaitForSeconds(delay);
        upperBodyAnimator.SetBool(paramName, false);
        upperBodyAnimator.SetBool("IsIdle", true);
    }

    public void TriggerHeavyPunchAnimation()
    {
        upperBodyAnimator.SetBool("IsHeavyPunch", true);
        upperBodyAnimator.SetBool("IsIdle", false);
        StartCoroutine(ResetAnimationAfterDelay("IsHeavyPunch", heavyPunchAnimDuration));
    }
}