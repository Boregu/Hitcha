using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    [Header("Components")]
    public Animator parentAnimator; // Whole-body Animator (for Uppercut, Death, etc.)
    public Animator upperBodyAnimator; // Upper-body Animator (for Punch, HeavyPunch, Shooting)
    public SpriteRenderer spriteRenderer;
    private bora movement;
    private float lastNonZeroInput; // Track last horizontal input

    [Header("Punch Settings")]
    public float heavyPunchMomentum = 5f; // Velocity boost for heavy punch
    private bool isPerformingAction = false; // Prevent overlapping animations

    void Start()
    {
        movement = GetComponent<bora>();

        if (parentAnimator == null)
            parentAnimator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (upperBodyAnimator == null)
        {
            Debug.LogError("Upper Body Animator not assigned!");
        }
    }

    void LateUpdate()
    {
        if (parentAnimator == null || movement == null || upperBodyAnimator == null) return;

        // Handle whole-body animations (movement, uppercut, death, etc.)
        HandleWholeBodyAnimations();

        // Handle upper body animations (combat, shooting)
        HandleUpperBodyAnimations();
    }

    void HandleWholeBodyAnimations()
    {
        // Get current horizontal input
        float currentInput = Input.GetAxisRaw("Horizontal");
        if (currentInput != 0)
        {
            lastNonZeroInput = currentInput;
        }

        // **Movement Animations**
        parentAnimator.SetBool("IsRun", Mathf.Abs(movement.Velocity.x) > 0.1f && movement.IsGrounded);
        parentAnimator.SetBool("IsJump", !movement.IsGrounded);
        parentAnimator.SetBool("IsDash", movement.IsDashing);
        parentAnimator.SetBool("IsIdle", Mathf.Abs(movement.Velocity.x) < 0.1f && movement.IsGrounded);

        // Wall slide animations
        parentAnimator.SetBool("IsWallSlideLeft", movement.IsWallSliding && movement.IsWallLeft);
        parentAnimator.SetBool("IsWallSlideRight", movement.IsWallSliding && movement.IsWallRight);

        // Flip sprite based on last input direction
        if (lastNonZeroInput != 0)
        {
            spriteRenderer.flipX = lastNonZeroInput < 0;
        }

        // Uppercut (Whole body, "E")
        if (!isPerformingAction && Input.GetKeyDown(KeyCode.E))
        {
            parentAnimator.SetTrigger("Uppercut");
            StartAction();
        }
    }

    void HandleUpperBodyAnimations()
    {
        if (isPerformingAction) return; // Prevent overlapping actions

        // Punch (e.g., mapped to "F")
        if (Input.GetKeyDown(KeyCode.F))
        {
            upperBodyAnimator.SetBool("Punch", true);
            StartAction(() => upperBodyAnimator.SetBool("Punch", false));
        }

        // Heavy Punch (Right Mouse Button)
        if (Input.GetMouseButtonDown(1))
        {
            upperBodyAnimator.SetBool("HeavyPunch", true);
            ApplyHeavyPunchMomentum();
            StartAction(() => upperBodyAnimator.SetBool("HeavyPunch", false));
        }

        // Shooting (Left Mouse Button)
        if (Input.GetMouseButtonDown(0))
        {
            upperBodyAnimator.SetBool("Shooting", true);
            StartAction(() => upperBodyAnimator.SetBool("Shooting", false));
            HandleShooting(); // Placeholder for shooting logic
        }
    }

    void StartAction(System.Action onActionComplete = null)
    {
        isPerformingAction = true; // Prevent overlapping actions
        Invoke("EndAction", 0.5f); // Adjust duration to match animation length

        // Reset the action-specific bool after the animation ends
        if (onActionComplete != null)
            Invoke(nameof(CompleteAction), 0.5f);
    }

    void CompleteAction()
    {
        isPerformingAction = false;
    }

    void EndAction()
    {
        isPerformingAction = false;
    }

    void ApplyHeavyPunchMomentum()
    {
        // Apply velocity boost in the current facing direction
        float direction = spriteRenderer.flipX ? -1 : 1;
        movement.GetComponent<Rigidbody2D>().linearVelocity += new Vector2(direction * heavyPunchMomentum, 0);
    }

    void HandleShooting()
    {
        // Placeholder for shooting logic
        Debug.Log("Shooting action triggered!");
    }
}
