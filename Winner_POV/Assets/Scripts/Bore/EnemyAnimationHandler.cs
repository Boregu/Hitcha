using UnityEngine;
using Pathfinding; // Make sure to import A* Pathfinding Project

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(Seeker))]
public class EnemyAnimationHandler : MonoBehaviour
{
    private Animator animator;
    private AIPath aiPath;
    private Seeker seeker;
    private SpriteRenderer spriteRenderer;

    // Animation parameter names
    private readonly string IS_WALKING = "IsWalking";
    private readonly string IS_ATTACKING = "IsAttacking";
    private readonly string IS_HURT = "IsHurt";
    private readonly string TRIGGER_DIE = "Die";

    [Header("Enemy Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public Transform target; // Usually the player
    public bool isFacingRight = true;

    // State tracking
    private bool isAttacking = false;
    private bool isDead = false;
    private float lastAttackTime;

    private void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        aiPath = GetComponent<AIPath>();
        seeker = GetComponent<Seeker>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (target == null)
        {
            // Try to find player if target not set
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // Initialize A* path
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
    }

    private void UpdatePath()
    {
        if (seeker.IsDone() && target != null && !isDead)
        {
            seeker.StartPath(transform.position, target.position);
        }
    }

    private void Update()
    {
        if (isDead) return;

        // Update movement animation
        bool isMoving = aiPath.velocity.magnitude > 0.1f;
        animator.SetBool(IS_WALKING, isMoving);

        // Handle facing direction
        if (isMoving)
        {
            isFacingRight = aiPath.velocity.x > 0;
            spriteRenderer.flipX = !isFacingRight;
        }

        // Check for attack
        if (target != null && !isAttacking)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (distanceToTarget <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                StartAttack();
            }
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        animator.SetBool(IS_ATTACKING, true);
        lastAttackTime = Time.time;
        
        // Stop movement during attack
        aiPath.canMove = false;
    }

    // Called via Animation Event at the end of attack animation
    public void OnAttackEnd()
    {
        isAttacking = false;
        animator.SetBool(IS_ATTACKING, false);
        aiPath.canMove = true;
    }

    public void TakeHit(float damage)
    {
        if (isDead) return;

        // Play hurt animation
        animator.SetBool(IS_HURT, true);

        // TODO: Add health system and check for death
        // if (health <= 0) Die();
    }

    // Called via Animation Event at the end of hurt animation
    public void OnHurtEnd()
    {
        animator.SetBool(IS_HURT, false);
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        aiPath.canMove = false;
        animator.SetTrigger(TRIGGER_DIE);

        // Disable components
        aiPath.enabled = false;
        seeker.enabled = false;

        // Optional: Add death effects, drop items, etc.
    }

    // Called via Animation Event at the end of death animation
    public void OnDeathEnd()
    {
        // Optional: Destroy the enemy or handle pooling
        Destroy(gameObject);
    }

    // Optional: Visualize attack range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
} 