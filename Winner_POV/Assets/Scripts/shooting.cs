using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab for the projectile
    public GameObject firePoint; // GameObject for the fire point
    public float shootInterval = 2f; // Time between each shot
    public float projectileSpeed = 10f; // Speed of the projectile
    public float projectileLifetime = 5f; // Time before the projectile disappears
    public float aggroRange = 10f; // Aggro range for the enemy to start shooting
    public float shootYTolerance = 0.5f; // How much Y-axis tolerance the enemy has to be aligned with the player

    private GameObject player; // Reference to the player
    private float lastShotTime; // Time of the last shot

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player == null || firePoint == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Check if the player is within aggro range and if the enemy is aligned with the player on the Y-axis
        if (distanceToPlayer <= aggroRange && Mathf.Abs(transform.position.y - player.transform.position.y) <= shootYTolerance)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // Check if enough time has passed to shoot again
        if (Time.time - lastShotTime >= shootInterval)
        {
            lastShotTime = Time.time;

            // Instantiate the projectile
            GameObject projectile = Instantiate(projectilePrefab, firePoint.transform.position, Quaternion.identity);

            // Calculate the direction towards the player on the X-axis (purely horizontal)
            Vector2 direction = new Vector2(player.transform.position.x > transform.position.x ? 1 : -1, 0);

            // Log the direction for debugging purposes
            Debug.Log("Shooting in direction: " + direction);

            // Set the projectile's velocity using the direction and speed
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed; // Apply the velocity
            }

            // Destroy the projectile after its lifetime
            Destroy(projectile, projectileLifetime);
        }
    }
}
