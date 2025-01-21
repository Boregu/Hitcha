using UnityEngine;
using UnityEngine.Rendering.Universal; // For Light2D

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 40;
    public float knockbackForce = 5f;

    [Header("Uppercut Settings")]
    public float uppercutForce = 15f;
    public float uppercutEnemyMultiplier = 1.2f;

    [Header("Heavy Punch Settings")]
    public float heavyPunchMaxChargeTime = 2f;
    public float heavyPunchMinXVelocity = 5f;
    public float heavyPunchMaxXVelocity = 20f;
    public float heavyPunchYVelocity = 5f;
    public float heavyPunchKnockbackMultiplier = 1.5f;
    public float boostDuration = 0.5f;

    [Header("Lights for Heavy Punch")]
    public Light2D chargeLight;
    public Light2D flashLight;
    public float maxChargeLightIntensity = 2f;
    public float flashLightIntensity = 2f;
    public float flashBlinkSpeed = 0.1f;

    [Header("Hitbox Settings")]
    public GameObject hitbox; // Reference to the hitbox GameObject
    public float hitboxFlipOffset = 1f;

    [SerializeField] private bool playerIsFacingRight;

    private Rigidbody2D rb;
    private RotateUpperBodyTowardMouse upperBodyRotation;
    private float nextAttackTime = 0f;

    private bool isChargingHeavyPunch = false;
    private bool isHeavyPunchFullyCharged = false;
    private float chargeTimeElapsed = 0f;
    private bool isBoosting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        upperBodyRotation = GetComponent<RotateUpperBodyTowardMouse>();

        if (upperBodyRotation == null)
        {
            Debug.LogError("RotateUpperBodyTowardMouse component not found! Ensure it is attached to the same GameObject as PlayerAttack.");
        }

        if (chargeLight != null) chargeLight.intensity = 0f;
        if (flashLight != null) flashLight.intensity = 0f;

        if (hitbox == null)
        {
            Debug.LogWarning("Hitbox GameObject is not assigned! Please assign it in the Inspector.");
        }
    }

    void Update()
    {
        if (upperBodyRotation != null)
        {
            playerIsFacingRight = upperBodyRotation.IsFacingRight;
            Debug.Log($"Player is facing right: {playerIsFacingRight}");
        }

        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0)) // Left-click for normal attack
            {
                NormalAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
            else if (Input.GetKeyDown(KeyCode.E)) // Uppercut on E
            {
                UppercutAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
            else if (Input.GetMouseButtonDown(1)) // Right-click to charge heavy punch
            {
                StartHeavyPunchCharge();
            }
            else if (Input.GetMouseButtonUp(1) && isChargingHeavyPunch) // Release heavy punch
            {
                PerformHeavyPunch();
            }
        }

        if (isChargingHeavyPunch)
        {
            HandleHeavyPunchCharge();
        }

        FlipHitbox();
    }

    void NormalAttack()
    {
        Debug.Log("Normal Attack!");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, uppercutForce);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Uppercut Hit " + enemy.name);

            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.linearVelocity = new Vector2(enemyRb.linearVelocity.x, uppercutForce * uppercutEnemyMultiplier);
            }
        }
    }

    void StartHeavyPunchCharge()
    {
        Debug.Log("Charging Heavy Punch...");
        isChargingHeavyPunch = true;
        chargeTimeElapsed = 0f;
        isHeavyPunchFullyCharged = false;

        if (chargeLight != null) chargeLight.intensity = 0f;
    }

    void HandleHeavyPunchCharge()
    {
        chargeTimeElapsed += Time.deltaTime;

        if (chargeLight != null)
            chargeLight.intensity = Mathf.Clamp01(chargeTimeElapsed / heavyPunchMaxChargeTime) * maxChargeLightIntensity;

        if (chargeTimeElapsed >= heavyPunchMaxChargeTime && !isHeavyPunchFullyCharged)
        {
            Debug.Log("Heavy Punch Fully Charged!");
            isHeavyPunchFullyCharged = true;

            if (flashLight != null)
                StartCoroutine(FlashLightCoroutine());
        }
    }

    void PerformHeavyPunch()
    {
        Debug.Log("Heavy Punch Released!");
        isChargingHeavyPunch = false;

        if (chargeLight != null) chargeLight.intensity = 0f;
        if (flashLight != null) flashLight.intensity = 0f;

        float chargePercentage = Mathf.Clamp01(chargeTimeElapsed / heavyPunchMaxChargeTime);
        float xVelocity = Mathf.Lerp(heavyPunchMinXVelocity, heavyPunchMaxXVelocity, chargePercentage);
        float yVelocity = heavyPunchYVelocity;

        xVelocity = playerIsFacingRight ? xVelocity : -xVelocity;

        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        isBoosting = true;

        Invoke("EndBoost", boostDuration);
    }

    void FlipHitbox()
    {
        if (hitbox == null) return;

        Vector3 hitboxPosition = hitbox.transform.localPosition;
        hitboxPosition.x = playerIsFacingRight ? Mathf.Abs(hitboxFlipOffset) : -Mathf.Abs(hitboxFlipOffset);
        hitbox.transform.localPosition = hitboxPosition;
    }

    System.Collections.IEnumerator FlashLightCoroutine()
    {
        while (isHeavyPunchFullyCharged && isChargingHeavyPunch)
        {
            if (flashLight != null)
            {
                flashLight.intensity = flashLightIntensity;
                yield return new WaitForSeconds(flashBlinkSpeed);
                flashLight.intensity = 0f;
                yield return new WaitForSeconds(flashBlinkSpeed);
            }
        }
    }
}
