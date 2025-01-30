using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Prefab for the projectile
    public GameObject firePoint; // GameObject for the fire point
    public float shootInterval = 0.5f; // Time between each shot
    public float projectileSpeed = 10f; // Speed of the projectile
    public float projectileLifetime = 5f; // Time before the projectile disappears
    
    [Header("Visual Effects")]
    public GameObject muzzleFlashLight; // Reference to the light GameObject
    public float flashLightDuration = 0.05f; // How long the flash light stays on
    public GameObject muzzleFlashParticlePrefab; // Particle effect prefab for shooting
    public GameObject hitEffectPrefab; // Particle effect for when projectile hits something
    
    private float lastShotTime; // Time of the last shot
    private float flashLightEndTime; // When to turn off the flash light

    void Start()
    {
        // Ensure flash light starts off
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.SetActive(false);
        }
    }

    void Update()
    {
        if (firePoint == null)
            return;

        // Handle flash light timing
        if (muzzleFlashLight != null && muzzleFlashLight.activeSelf && Time.time >= flashLightEndTime)
        {
            muzzleFlashLight.SetActive(false);
        }

        // Check for input and shoot
        if (Input.GetButtonDown("Fire1")) // Changed from GetButton to GetButtonDown for single shots
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

            // Handle muzzle flash light
            if (muzzleFlashLight != null)
            {
                muzzleFlashLight.SetActive(true);
                flashLightEndTime = Time.time + flashLightDuration;
            }

            // Handle muzzle flash particle effect
            if (muzzleFlashParticlePrefab != null)
            {
                // Instantiate the particle system as a child of firePoint
                GameObject muzzleFlash = Instantiate(muzzleFlashParticlePrefab, firePoint.transform.position, firePoint.transform.rotation, firePoint.transform);
                
                // Get the particle system component
                ParticleSystem particleSystem = muzzleFlash.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    // Make sure the particle system is using local space for proper positioning
                    var mainModule = particleSystem.main;
                    mainModule.simulationSpace = ParticleSystemSimulationSpace.Local;
                    
                    float particleDuration = mainModule.duration;
                    // Detach from parent before destroying to prevent visual glitches
                    muzzleFlash.transform.SetParent(null, true);
                    Destroy(muzzleFlash, particleDuration);
                }
            }

            // Calculate the direction based on mouse position
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            Vector2 direction = ((Vector2)(mousePosition - firePoint.transform.position)).normalized;

            // Set up the projectile
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed;
            }

            // Add a ProjectileCollision component to handle hit effects
            ProjectileCollision collisionHandler = projectile.AddComponent<ProjectileCollision>();
            collisionHandler.hitEffectPrefab = hitEffectPrefab;

            // Destroy the projectile after its lifetime
            Destroy(projectile, projectileLifetime);
        }
    }
}

// New class to handle projectile collisions
public class ProjectileCollision : MonoBehaviour
{
    public GameObject hitEffectPrefab;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Create hit effect at the point of collision
        if (hitEffectPrefab != null)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            GameObject hitEffect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
            Destroy(hitEffect, 1f); // Destroy the hit effect after 1 second
        }

        // Destroy the projectile
        Destroy(gameObject);
    }
}
