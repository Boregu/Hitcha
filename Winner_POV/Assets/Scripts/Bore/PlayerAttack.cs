using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RotateUpperBodyTowardMouse upperBodyRotation;

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
    public float heavyPunchFullyChargedSpeed = 35f;
    public float chargeSpeedMultiplierMin = 0.3f;
    public float chargeSpeedMultiplierMax = 0.8f;
    public float heavyPunchYVelocity = 5f;
    public float heavyPunchKnockbackMultiplier = 1.5f;
    public float boostDuration = 0.5f;
    public bool bypassVelocityLimit = true;
    public float chargingMovementSpeedMultiplier = 0.3f;
    public float heavyPunchMaxXVelocityCap = 40f;
    public float existingYVelocityMultiplier = 0.2f;
    public float yToXVelocityTransferMultiplier = 1f;
    public bool allowHeavyPunchDuringUppercut = false;

    [Header("Lights for Heavy Punch")]
    public Light2D chargeLight;
    public Light2D flashLight;
    public float maxChargeLightIntensity = 2f;
    public float flashLightIntensity = 2f;
    public float flashBlinkSpeed = 0.1f;

    [Header("Hitbox Settings")]
    public GameObject hitbox;
    public float hitboxFlipOffset = 1f;

    [SerializeField] private bool playerIsFacingRight;

    private Rigidbody2D rb;
    private float nextAttackTime = 0f;
    private bora movement;

    private bool isChargingHeavyPunch = false;
    private bool isHeavyPunchFullyCharged = false;
    private float chargeTimeElapsed = 0f;
    private float originalMoveSpeed;
    private float originalAirMaxSpeed;

    // Public properties for animation states
    public bool IsChargingHeavyPunch => isChargingHeavyPunch;
    public bool IsHeavyPunchFullyCharged => isHeavyPunchFullyCharged;
    public bool IsUppercutting { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<bora>();

        if (upperBodyRotation == null)
        {
            Debug.LogError("RotateUpperBodyTowardMouse reference not set! Please assign it in the Inspector.");
        }

        if (chargeLight != null) chargeLight.intensity = 0f;
        if (flashLight != null) flashLight.intensity = 0f;

        if (hitbox == null)
        {
            Debug.LogWarning("Hitbox GameObject is not assigned! Please assign it in the Inspector.");
        }

        if (movement == null)
        {
            Debug.LogError("bora movement component not found! Ensure it is attached to the same GameObject.");
        }
        else
        {
            originalMoveSpeed = movement.moveSpeed;
            originalAirMaxSpeed = movement.airMaxSpeed;
        }
    }

    void Update()
    {
        if (upperBodyRotation != null)
        {
            playerIsFacingRight = upperBodyRotation.IsFacingRight;
        }

        // Handle attack inputs
        if (Input.GetMouseButtonDown(0))  // Left Click - Shoot
        {
            if (Time.time >= nextAttackTime)
            {
                Shoot();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else if (Input.GetMouseButtonDown(1))  // Right Click - Punch
        {
            if (Time.time >= nextAttackTime)
            {
                NormalAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))  // E key - Uppercut
        {
            if (Time.time >= nextAttackTime)
            {
                UppercutAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))  // Q key - Heavy Punch
        {
            StartHeavyPunchCharge();
        }
        else if (Input.GetKeyUp(KeyCode.Q) && isChargingHeavyPunch)
        {
            PerformHeavyPunch();
        }

        if (isChargingHeavyPunch)
        {
            HandleHeavyPunchCharge();
        }

        FlipHitbox();
    }

    void Shoot()
    {
        // Implement shooting logic here
    }

    void NormalAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
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
        IsUppercutting = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, uppercutForce);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.linearVelocity = new Vector2(enemyRb.linearVelocity.x, uppercutForce * uppercutEnemyMultiplier);
            }
        }

        Invoke("ResetUppercut", 0.1f);
    }

    void ResetUppercut()
    {
        IsUppercutting = false;
    }

    void StartHeavyPunchCharge()
    {
        isChargingHeavyPunch = true;
        chargeTimeElapsed = 0f;
        isHeavyPunchFullyCharged = false;

        if (movement != null)
        {
            movement.moveSpeed = originalMoveSpeed * chargingMovementSpeedMultiplier;
            movement.airMaxSpeed = originalAirMaxSpeed * chargingMovementSpeedMultiplier;
        }

        if (chargeLight != null) chargeLight.intensity = 0f;
    }

    void HandleHeavyPunchCharge()
    {
        chargeTimeElapsed += Time.deltaTime;

        if (chargeLight != null)
            chargeLight.intensity = Mathf.Clamp01(chargeTimeElapsed / heavyPunchMaxChargeTime) * maxChargeLightIntensity;

        if (chargeTimeElapsed >= heavyPunchMaxChargeTime && !isHeavyPunchFullyCharged)
        {
            isHeavyPunchFullyCharged = true;

            if (flashLight != null)
                StartCoroutine(FlashLightCoroutine());
        }
    }

    void PerformHeavyPunch()
    {
        isChargingHeavyPunch = false;

        if (movement != null)
        {
            movement.moveSpeed = originalMoveSpeed;
            movement.airMaxSpeed = originalAirMaxSpeed;
            movement.SetSpeedLimitBypass(true, heavyPunchMaxXVelocityCap);
        }

        if (chargeLight != null) chargeLight.intensity = 0f;
        if (flashLight != null) flashLight.intensity = 0f;

        float chargePercentage = Mathf.Clamp01(chargeTimeElapsed / heavyPunchMaxChargeTime);
        float xVelocity;
        
        if (isHeavyPunchFullyCharged)
        {
            xVelocity = heavyPunchFullyChargedSpeed;
        }
        else
        {
            float chargeMultiplier = Mathf.Lerp(chargeSpeedMultiplierMin, chargeSpeedMultiplierMax, chargePercentage);
            xVelocity = heavyPunchFullyChargedSpeed * chargeMultiplier;
        }

        float yVelocity = heavyPunchYVelocity;
        Vector2 currentVelocity = rb.linearVelocity;
        float transferredYVelocity = Mathf.Abs(currentVelocity.y) * yToXVelocityTransferMultiplier;
        float directionMultiplier = playerIsFacingRight ? 1 : -1;
        
        float newXVelocity = (xVelocity + transferredYVelocity) * directionMultiplier;
        float newYVelocity = currentVelocity.y * existingYVelocityMultiplier + yVelocity;

        rb.linearVelocity = new Vector2(newXVelocity, newYVelocity);

        Invoke("EndBoost", boostDuration);
    }

    void EndBoost()
    {
        if (movement != null)
        {
            movement.moveSpeed = originalMoveSpeed;
            movement.airMaxSpeed = originalAirMaxSpeed;
            movement.SetSpeedLimitBypass(false, originalAirMaxSpeed);
        }
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