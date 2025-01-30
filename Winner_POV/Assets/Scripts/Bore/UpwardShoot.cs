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
    private Vector3 currentOffset;
    private Vector3 targetOffset;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (upperBody == null) Debug.LogError("UpperBody Transform not assigned!");
        if (targetGameObject == null) Debug.LogError("Target GameObject not assigned!");
        if (animationHandler == null) Debug.LogError("AnimationHandler not assigned!");

        defaultLocalPosition = targetGameObject.localPosition;
        playerAttack = GetComponent<PlayerAttack>();
        currentOffset = Vector3.zero;
        targetOffset = Vector3.zero;
    }

    void LateUpdate()
    {
        if (mainCamera == null || upperBody == null || targetGameObject == null || animationHandler == null) return;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 0;
        mousePosition = mainCamera.ScreenToWorldPoint(mousePosition);

        Vector3 aimOrigin = upperBody.position + aimOriginOffset;
        Vector3 direction = (mousePosition - aimOrigin).normalized;

        // For non-uppercut states, determine facing direction based on mouse position
        if (!animationHandler.IsUppercutting)
        {
            IsFacingRight = direction.x > 0;
        }
        else
        {
            // For uppercut, use the last pressed A/D key
            IsFacingRight = animationHandler.LastNonZeroInput >= 0;
        }

        // Calculate angle for rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + aimOffsetAngle;

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
        else
        {
            // Face straight left or right
            float straightAngle = IsFacingRight ? 0 : 180;
            upperBody.rotation = Quaternion.Euler(0, 0, straightAngle);
        }

        // Handle scaling
        upperBody.localScale = new Vector3(IsFacingRight ? 1 : -1, 1, 1);

        // Apply offset immediately after determining the state
        ApplyAnimationOffset();
    }

    void ApplyAnimationOffset()
    {
        // Check states in priority order and directly set the offset
        if (playerAttack != null && playerAttack.IsChargingHeavyPunch)
        {
            targetOffset = IsFacingRight ? chargingHeavyPunchRightOffset : chargingHeavyPunchLeftOffset;
        }
        else if (animationHandler.IsHeavyPunching)
        {
            targetOffset = IsFacingRight ? heavyPunchRightOffset : heavyPunchLeftOffset;
        }
        else if (animationHandler.IsUppercutting)
        {
            targetOffset = IsFacingRight ? uppercutRightOffset : uppercutLeftOffset;
        }
        else if (animationHandler.IsPunching)
        {
            targetOffset = IsFacingRight ? punchRightOffset : punchLeftOffset;
        }
        else if (animationHandler.IsShooting)
        {
            targetOffset = IsFacingRight ? shootRightOffset : shootLeftOffset;
        }
        else // Idle state
        {
            targetOffset = IsFacingRight ? idleRightOffset : idleLeftOffset;
        }

        // Directly set the position
        targetGameObject.localPosition = defaultLocalPosition + targetOffset;
    }

    public void ResetToDefaultPosition()
    {
        if (targetGameObject != null)
        {
            targetGameObject.localPosition = defaultLocalPosition;
        }
    }
}