using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    private bora movement;
    private float lastNonZeroInput;  // Track last horizontal input

    void Start()
    {
        movement = GetComponent<bora>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (animator == null || movement == null) return;

        // Get current horizontal input
        float currentInput = Input.GetAxisRaw("Horizontal");
        if (currentInput != 0)
        {
            lastNonZeroInput = currentInput;
        }

        // Basic movement animations
        animator.SetBool("IsRun", Mathf.Abs(movement.Velocity.x) > 0.1f && movement.IsGrounded);
        animator.SetBool("IsJump", !movement.IsGrounded);
        animator.SetBool("IsDash", movement.IsDashing);
        animator.SetBool("IsIdle", Mathf.Abs(movement.Velocity.x) < 0.1f && movement.IsGrounded);

        // Wall slide animations
        animator.SetBool("IsWallSlideLeft", movement.IsWallSliding && movement.IsWallLeft);
        animator.SetBool("IsWallSlideRight", movement.IsWallSliding && movement.IsWallRight);

        // Facing direction based on last input
        animator.SetBool("IsFacingLeft", lastNonZeroInput < 0);
        animator.SetBool("IsFacingRight", lastNonZeroInput > 0);

        // Flip sprite based on last input direction
        if (lastNonZeroInput != 0)
        {
            spriteRenderer.flipX = lastNonZeroInput < 0;
        }
    }
} 