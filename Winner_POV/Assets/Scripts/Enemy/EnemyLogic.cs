using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyAnimationHandler))]
public class EnemyLogic : MonoBehaviour
{
    private Rigidbody2D rb;
    private EnemyAnimationHandler animHandler;

    [Header("Movement Settings")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f; // Speed when chasing player
    public float chaseAnimationSpeedMultiplier = 1.5f; // Animation speed multiplier when chasing
    public bool isFacingRight = true;

    [Header("Patrol Points")]
    public Vector2 pointA; // First patrol point position
    public Vector2 pointB; // Second patrol point position
    public float waitTimeAtPoints = 2f; // How long to wait at each point
    public Color gizmoColor = Color.blue; // Color for the patrol point visualization
    public Vector2 patrolPointSize = new Vector2(1f, 2f); // Size of the patrol point boxes

    [Header("Detection and Combat")]
    public float initialDetectionRange = 8f; // Range to first detect player
    public float loseDetectionRange = 12f; // Range at which player is lost (larger than detection)
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float maxHealth = 100f;
    public Transform target; // Usually the player
    public LayerMask playerLayer; // Layer for player detection

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.3f;

    [Header("Vision Settings")]
    public int numberOfRays = 8; // Number of rays to cast
    public float visionConeAngle = 90f; // Total angle of the vision cone in degrees
    public bool showVisionRays = true; // Toggle for debug rays

    // State tracking
    private bool isAttacking = false;
    private bool isDead = false;
    private float lastAttackTime;
    private bool isGrounded;
    private float currentHealth;
    private bool movingTowardsB = true;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool hasDetectedPlayer = false; // Tracks if player has been detected
    private Vector2 lastKnownPlayerPosition;
    private bool canAttack = true; // New variable to track if we can start a new attack

    private void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        animHandler = GetComponent<EnemyAnimationHandler>();

        // Setup rigidbody for ground movement
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Initialize health
        currentHealth = maxHealth;

        // Set default patrol points if they're at zero
        if (pointA == Vector2.zero && pointB == Vector2.zero)
        {
            pointA = (Vector2)transform.position + Vector2.left * 3f;
            pointB = (Vector2)transform.position + Vector2.right * 3f;
        }

        // Try to find player if target is not set
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    private void Update()
    {
        if (isDead) return;

        // Check if grounded
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        if (!isGrounded) 
        {
            StopMovement();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // First check if we should lose detection (larger range)
        if (hasDetectedPlayer && (distanceToPlayer > loseDetectionRange || !CheckLineOfSight(target.position, distanceToPlayer)))
        {
            hasDetectedPlayer = false;
        }
        // Then check if we should gain detection (smaller range)
        else if (!hasDetectedPlayer && distanceToPlayer <= initialDetectionRange && CheckLineOfSight(target.position, distanceToPlayer))
        {
            hasDetectedPlayer = true;
        }

        // If player is detected, handle attack and chase states
        if (hasDetectedPlayer)
        {
            // STATE 1: If player is in attack range, ATTACK
            if (distanceToPlayer <= attackRange && CheckLineOfSight(target.position, distanceToPlayer))
            {
                if (!isAttacking && canAttack && Time.time >= lastAttackTime + attackCooldown)
                {
                    StartAttack();
                }
                return;
            }

            // STATE 2: CHASE if not attacking
            if (!isAttacking)
            {
                // Move towards player
                Vector2 direction = (target.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
                isFacingRight = direction.x > 0;
                animHandler.SetWalking(true, isFacingRight, chaseAnimationSpeedMultiplier);
            }
            return;
        }

        // STATE 3: PATROL if player not detected
        if (!isAttacking)
        {
            HandlePatrolling();
        }
    }

    private bool CheckLineOfSight(Vector2 targetPos, float distance)
    {
        Vector2 baseDirection = (targetPos - (Vector2)transform.position).normalized;
        Vector2 origin = transform.position;
        
        // Calculate the angles for each ray
        float angleStep = visionConeAngle / (numberOfRays - 1);
        float startAngle = -visionConeAngle / 2f;

        // Cast center ray first
        if (CastRay(origin, baseDirection, distance))
        {
            return true;
        }

        // Cast rays in a cone pattern
        for (int i = 0; i < numberOfRays; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            float radians = currentAngle * Mathf.Deg2Rad;
            
            // Rotate the base direction vector by the current angle
            Vector2 direction = new Vector2(
                baseDirection.x * Mathf.Cos(radians) - baseDirection.y * Mathf.Sin(radians),
                baseDirection.x * Mathf.Sin(radians) + baseDirection.y * Mathf.Cos(radians)
            );

            if (CastRay(origin, direction, distance))
            {
                return true;
            }
        }

        return false;
    }

    private bool CastRay(Vector2 origin, Vector2 direction, float distance)
    {
        // Draw debug ray if enabled
        if (showVisionRays)
        {
            Debug.DrawRay(origin, direction * distance, Color.yellow);
        }

        // Cast ray
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, groundLayer);
        
        // Return true if we hit nothing or hit the player
        return hit.collider == null || hit.collider.transform == target;
    }

    private void HandlePatrolling()
    {
        if (isWaiting)
        {
            // Handle waiting at points
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                movingTowardsB = !movingTowardsB;
            }
            StopMovement();
        }
        else
        {
            // Get current target point
            Vector2 targetPoint = movingTowardsB ? pointB : pointA;
            float directionX = targetPoint.x - transform.position.x;
            float normalizedDirection = Mathf.Sign(directionX);

            // Check if we've reached the target point
            if (Mathf.Abs(directionX) < 0.1f)
            {
                isWaiting = true;
                waitTimer = waitTimeAtPoints;
                StopMovement();
            }
            else
            {
                // Move towards target point with normal animation speed
                rb.linearVelocity = new Vector2(normalizedDirection * patrolSpeed, rb.linearVelocity.y);
                isFacingRight = normalizedDirection > 0;
                animHandler.SetWalking(true, isFacingRight, 1f);
            }
        }
    }

    private void StopMovement()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animHandler.SetWalking(false, isFacingRight, 1f);
    }

    private void StartAttack()
    {
        isAttacking = true;
        canAttack = false; // Prevent new attacks until animation finishes
        animHandler.StartAttackAnimation();
        lastAttackTime = Time.time;
        Debug.Log($"Enemy {gameObject.name}: Attack Started");
        
        // Stop horizontal movement during attack
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // Called via Animation Event at the end of attack animation
    public void OnAttackEnd()
    {
        Debug.Log($"Enemy {gameObject.name}: OnAttackEnd called");
        isAttacking = false;
        canAttack = true;
        animHandler.StopAttackAnimation();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animHandler.PlayHurtAnimation();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Called via Animation Event at the end of hurt animation
    public void OnHurtEnd()
    {
        animHandler.StopHurtAnimation();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        rb.linearVelocity = Vector2.zero;
        animHandler.PlayDeathAnimation();

        // Disable components
        rb.simulated = false;
        enabled = false;
    }

    // Called via Animation Event at the end of death animation
    public void OnDeathEnd()
    {
        // Destroy the enemy
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Draw patrol points and path
        Gizmos.color = gizmoColor;
        DrawPatrolPointBox(pointA);
        DrawPatrolPointBox(pointB);
        Gizmos.DrawLine(pointA, pointB);

        #if UNITY_EDITOR
        if (Selection.activeGameObject == gameObject)
        #endif
        {
            // Draw detection ranges
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, initialDetectionRange);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, loseDetectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw ground check
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        }
    }

    private void DrawPatrolPointBox(Vector2 point)
    {
        // Calculate box corners
        Vector2 topLeft = new Vector2(point.x - patrolPointSize.x/2, point.y + patrolPointSize.y/2);
        Vector2 topRight = new Vector2(point.x + patrolPointSize.x/2, point.y + patrolPointSize.y/2);
        Vector2 bottomLeft = new Vector2(point.x - patrolPointSize.x/2, point.y - patrolPointSize.y/2);
        Vector2 bottomRight = new Vector2(point.x + patrolPointSize.x/2, point.y - patrolPointSize.y/2);

        // Draw box outline
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
