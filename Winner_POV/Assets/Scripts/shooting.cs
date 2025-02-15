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

    [Header("Aiming")]
    public Camera mainCamera; // Reference to the main camera
    public float aimOffsetAngle = 0f;
    public Vector3 aimOriginOffset = Vector3.zero;
    
    private float lastShotTime; // Time of the last shot
    private float flashLightEndTime; // When to turn off the flash light
    private Vector2 aimDirection; // Store the current aim direction
    [SerializeField] public bool IsFacingRight = true;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Ensure flash light starts off
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (firePoint == null || mainCamera == null)
            return;

        // Update aim direction
        UpdateAimDirection();

        // Handle flash light timing
        if (muzzleFlashLight != null && muzzleFlashLight.activeSelf && Time.time >= flashLightEndTime)
        {
            muzzleFlashLight.SetActive(false);
        }

        // Check for input and shoot
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void UpdateAimDirection()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 0;
        mousePosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Calculate aim from the base position (y=0)
        Vector3 basePosition = transform.position;
        basePosition.y = transform.position.y - aimOriginOffset.y; // Subtract the visual offset for aiming calculation
        Vector3 direction = (mousePosition - basePosition).normalized;
        aimDirection = direction;

        // Calculate angle for rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + aimOffsetAngle;

        // Determine facing direction based on mouse position
        IsFacingRight = direction.x > 0;

        // Full rotation following cursor
        if (IsFacingRight)
        {
            firePoint.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // When facing left, we don't add 180 to the angle, just flip the scale
            firePoint.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Handle scaling - this will handle the flipping
        firePoint.transform.localScale = new Vector3(IsFacingRight ? 1 : -1, 1, 1);
    }

    private void Shoot()
    {
        // Check if enough time has passed to shoot again
        if (Time.time - lastShotTime >= shootInterval)
        {
            lastShotTime = Time.time;
            
            // Instantiate the projectile with the correct rotation
            GameObject projectile = Instantiate(projectilePrefab, firePoint.transform.position, firePoint.transform.rotation);

            // Handle muzzle flash light
            if (muzzleFlashLight != null)
            {
                muzzleFlashLight.SetActive(true);
                flashLightEndTime = Time.time + flashLightDuration;
            }

            // Handle muzzle flash particle effect
            if (muzzleFlashParticlePrefab != null)
            {
                float particleRotation = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                if (!IsFacingRight)
                {
                    particleRotation += 180f;
                }
                GameObject muzzleFlash = Instantiate(muzzleFlashParticlePrefab, firePoint.transform.position, 
                    Quaternion.Euler(0, 0, particleRotation));
                
                Destroy(muzzleFlash, 2f);
            }

            // Set up the projectile
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = aimDirection * projectileSpeed;
            }

            // Add a ProjectileCollision component to handle hit effects
            ProjectileCollision collisionHandler = projectile.AddComponent<ProjectileCollision>();
            collisionHandler.hitEffectPrefab = hitEffectPrefab;

            // Destroy the projectile after its lifetime
            Destroy(projectile, projectileLifetime);
        }
    }
}

// ProjectileCollision class remains unchanged
public class ProjectileCollision : MonoBehaviour
{
    public GameObject hitEffectPrefab;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hitEffectPrefab != null)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            GameObject hitEffect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
            Destroy(hitEffect, 1f);
        }

        Destroy(gameObject);
    }
}