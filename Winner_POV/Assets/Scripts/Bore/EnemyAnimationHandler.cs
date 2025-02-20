using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationHandler : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Animation parameter names
    private readonly string IS_WALKING = "IsWalking";
    private readonly string IS_IDLE = "IsIdle";
    private readonly string IS_ATTACKING = "IsAttacking";
    private readonly string IS_HURT = "IsHurt";
    private readonly string TRIGGER_DIE = "Die";
    private readonly string SPEED_MULTIPLIER = "SpeedMultiplier";

    private void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetWalking(bool isWalking, bool isFacingRight, float speedMultiplier = 1f)
    {
        animator.SetBool(IS_WALKING, isWalking);
        animator.SetBool(IS_IDLE, !isWalking);
        animator.SetFloat(SPEED_MULTIPLIER, isWalking ? speedMultiplier : 1f);
        spriteRenderer.flipX = !isFacingRight;
    }

    public void StartAttackAnimation()
    {
        animator.SetBool(IS_ATTACKING, true);
        animator.SetBool(IS_WALKING, false);
        animator.SetBool(IS_IDLE, false);
    }

    public void StopAttackAnimation()
    {
        animator.SetBool(IS_ATTACKING, false);
        animator.SetBool(IS_IDLE, true);
    }

    public void PlayHurtAnimation()
    {
        animator.SetBool(IS_HURT, true);
    }

    public void StopHurtAnimation()
    {
        animator.SetBool(IS_HURT, false);
    }

    public void PlayDeathAnimation()
    {
        animator.SetTrigger(TRIGGER_DIE);
    }
} 