using UnityEngine;

public class RotateUpperBodyTowardMouse : MonoBehaviour
{
    [Header("References")]
    public Transform upperBody;
    public Camera mainCamera;
    public AnimationHandler animationHandler;
    public Transform targetGameObject;

    [Header("Offsets for Animations (Right)")]
    public Vector3 idleRightOffset = Vector3.zero;
    public Vector3 shootRightOffset = Vector3.zero;
    public Vector3 punchRightOffset = Vector3.zero;
    public Vector3 uppercutRightOffset = Vector3.zero;
    public Vector3 heavyPunchRightOffset = Vector3.zero;
    public Vector3 chargingHeavyPunchRightOffset = Vector3.zero;

    [Header("Offsets for Animations (Left)")]
    public Vector3 idleLeftOffset = Vector3.zero;
    public Vector3 shootLeftOffset = Vector3.zero;
    public Vector3 punchLeftOffset = Vector3.zero;
    public Vector3 uppercutLeftOffset = Vector3.zero;
    public Vector3 heavyPunchLeftOffset = Vector3.zero;
    public Vector3 chargingHeavyPunchLeftOffset = Vector3.zero;

    [Header("Aim Settings")]
    public float aimOffsetAngle = 0f;
    public float aimProjectionDistance = 5f;
    public Vector3 aimOriginOffset = Vector3.zero;

    private Vector3 defaultLocalPosition;
    [SerializeField] public bool IsFacingRight;
    private PlayerAttack playerAttack;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (upperBody == null) Debug.LogError("UpperBody Transform not assigned!");
        if (targetGameObject == null) Debug.LogError("Target GameObject not assigned!");
        if (animationHandler == null) Debug.LogError("AnimationHandler not assigned!");

        defaultLocalPosition = targetGameObject.localPosition;
        playerAttack = GetComponent<PlayerAttack>();
    }

    void Update()
    {
        if (mainCamera == null || upperBody == null || targetGameObject == null || animationHandler == null) return;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 0;
        mousePosition = mainCamera.ScreenToWorldPoint(mousePosition);

        Vector3 aimOrigin = upperBody.position + aimOriginOffset;
        Vector3 direction = (mousePosition - aimOrigin).normalized;

        // Determine facing direction based on mouse position
        IsFacingRight = direction.x > 0;

        // Calculate angle for rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + aimOffsetAngle;

        // Only follow cursor during idle and shooting
        if (animationHandler.IsIdle || animationHandler.IsShooting)
        {
            // Full rotation following cursor
            if (IsFacingRight)
            {
                upperBody.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                upperBody.rotation = Quaternion.Euler(0, 0, angle + 180f);
            }
        }
        else if (animationHandler.IsUppercutting)
        {
            // Use A/D input direction for uppercut, but maintain upright orientation
            bool facingRight = animationHandler.LastNonZeroInput >= 0;
            float straightAngle = facingRight ? 0 : 180;
            upperBody.rotation = Quaternion.Euler(0, 0, straightAngle);
            
            // Only flip the scale, don't rotate upside down
            upperBody.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
            IsFacingRight = facingRight;
        }
        else
        {
            // Face straight left or right based on cursor for other attacks
            float straightAngle = IsFacingRight ? 0 : 180;
            upperBody.rotation = Quaternion.Euler(0, 0, straightAngle);
        }

        // Handle scaling for non-uppercut states
        if (!animationHandler.IsUppercutting)
        {
            upperBody.localScale = new Vector3(IsFacingRight ? 1 : -1, 1, 1);
        }

        ApplyAnimationOffset();
    }

    void ApplyAnimationOffset()
    {
        Vector3 offset = defaultLocalPosition;

        // Check states in priority order
        if (playerAttack != null && playerAttack.IsChargingHeavyPunch)
        {
            offset = IsFacingRight ? chargingHeavyPunchRightOffset : chargingHeavyPunchLeftOffset;
        }
        else if (animationHandler.IsHeavyPunching)
        {
            offset = IsFacingRight ? heavyPunchRightOffset : heavyPunchLeftOffset;
        }
        else if (animationHandler.IsUppercutting)
        {
            bool facingRight = animationHandler.LastNonZeroInput >= 0;
            offset = facingRight ? uppercutRightOffset : uppercutLeftOffset;
        }
        else if (animationHandler.IsPunching)
        {
            offset = IsFacingRight ? punchRightOffset : punchLeftOffset;
        }
        else if (animationHandler.IsShooting)
        {
            offset = IsFacingRight ? shootRightOffset : shootLeftOffset;
        }
        else // Idle state
        {
            offset = IsFacingRight ? idleRightOffset : idleLeftOffset;
        }

        targetGameObject.localPosition = Vector3.Lerp(
            targetGameObject.localPosition, 
            defaultLocalPosition + offset, 
            Time.deltaTime * 10f
        );
    }

    public void ResetToDefaultPosition()
    {
        if (targetGameObject != null)
        {
            targetGameObject.localPosition = defaultLocalPosition;
        }
    }
}