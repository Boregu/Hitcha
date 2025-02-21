using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 40;
    public float knockbackForce = 5f; // Force applied to enemies on normal hit

    [Header("Uppercut Settings")]
    public float uppercutForce = 15f; // Upward force applied to the player during an uppercut
    public float uppercutEnemyMultiplier = 1.2f; // Enemy goes higher than the player

    [Header("Dash Settings")]
    public float dashStunDuration = 0.5f; // How long enemies are stunned when dashed through
    public float dashKnockUpForce = 8f;   // Upward force applied to enemies when dashed through
    public float dashAttackRange = 1.0f;  // Dash attack detection range

    private Rigidbody2D rb;
    private float nextAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Reference to the Rigidbody2D
    }

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0)) // Left click for a normal attack
            {
                NormalAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
            else if (Input.GetMouseButtonDown(1)) // Right click for uppercut attack
            {
                UppercutAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void NormalAttack()
    {
        Debug.Log("Normal Attack!");

        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage and knock back enemies
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);

            // Damage enemy (if applicable)
            // enemy.GetComponent<Enemy>().TakeDamage(attackDamage);

            // Apply knockback
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    void UppercutAttack()
    {
        Debug.Log("Uppercut Attack!");

        // Apply upward force to the player
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, uppercutForce);

        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage and knock up enemies
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Uppercut Hit " + enemy.name);

            // Damage enemy
            // enemy.GetComponent<Enemy>().TakeDamage(attackDamage);

            // Apply upward force to the enemy
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.linearVelocity = new Vector2(enemyRb.linearVelocity.x, uppercutForce * uppercutEnemyMultiplier);
            }
        }
    }

    public void DashAttack()
    {
        Debug.Log("Dash Attack!");

        // Detect enemies in range of the dash
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, dashAttackRange, enemyLayers);

        // Stun and knock up enemies
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Dashed Through " + enemy.name);

            // Stun the enemy (if applicable)
            EnemyStun enemyStun = enemy.GetComponent<EnemyStun>();
            if (enemyStun != null)
            {
                enemyStun.Stun(dashStunDuration);
            }

            // Apply upward knock-up force
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.linearVelocity = new Vector2(enemyRb.linearVelocity.x, dashKnockUpForce);
            }
        }
    }

    // To visualize the attack range in the editor
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, dashAttackRange);
    }
}
