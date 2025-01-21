using UnityEngine;

public class RotateUpperBodyTowardMouse : MonoBehaviour
{
    [Header("References")]
    public Transform upperBody;
    public Camera mainCamera;
    public Animator animator;

    [Header("Offsets for Animations (Right)")]
    public Vector3 idleRightOffset = Vector3.zero;
    public Vector3 shootRightOffset = Vector3.zero;
    public Vector3 punchRightOffset = Vector3.zero;
    public Vector3 uppercutRightOffset = Vector3.zero;
    public Vector3 heavyPunchRightOffset = Vector3.zero;

    [Header("Offsets for Animations (Left)")]
    public Vector3 idleLeftOffset = Vector3.zero;
    public Vector3 shootLeftOffset = Vector3.zero;
    public Vector3 punchLeftOffset = Vector3.zero;
    public Vector3 uppercutLeftOffset = Vector3.zero;
    public Vector3 heavyPunchLeftOffset = Vector3.zero;

    [Header("Aim Settings")]
    public float aimOffsetAngle = 0f;
    public Transform targetGameObject;
    public float aimProjectionDistance = 5f;
    public Vector3 aimOriginOffset = Vector3.zero;

    private Vector3 defaultLocalPosition;

    [SerializeField] public bool IsFacingRight; // Public-facing bool for external access

    void Start()
    {
        if (mainCamera == null) Debug.LogError("Main Camera not assigned!");
        if (upperBody == null) Debug.LogError("UpperBody Transform not assigned!");
        if (targetGameObject == null) Debug.LogError("Target GameObject not assigned!");
        if (animator == null) Debug.LogError("Animator not assigned!");

        defaultLocalPosition = targetGameObject.localPosition;
    }

    void Update()
    {
        if (mainCamera == null || upperBody == null || targetGameObject == null || animator == null) return;

        // Get mouse position in world space
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 0;
        mousePosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Calculate direction from the upper body to the mouse
        Vector3 aimOrigin = upperBody.position + aimOriginOffset;
        Vector3 direction = (mousePosition - aimOrigin).normalized;

        // Determine the facing direction
        bool previousFacingRight = IsFacingRight;
        IsFacingRight = direction.x > 0;

        if (previousFacingRight != IsFacingRight)
        {
            Debug.Log($"Facing direction changed: IsFacingRight is now {IsFacingRight}");
        }

        // Rotate the upper body based on the facing direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + aimOffsetAngle;

        if (IsFacingRight)
        {
            upperBody.localScale = new Vector3(1, 1, 1); // Face right
            upperBody.rotation = Quaternion.Euler(0, 0, angle);
            ApplyAnimationOffset(true);
        }
        else
        {
            upperBody.localScale = new Vector3(-1, 1, 1); // Face left
            upperBody.rotation = Quaternion.Euler(0, 0, angle + 180f);
            ApplyAnimationOffset(false);
        }
    }

    void ApplyAnimationOffset(bool facingRight)
    {
        Vector3 offset = defaultLocalPosition;

        if (animator.GetBool("IsPunch"))
        {
            offset = facingRight ? punchRightOffset : punchLeftOffset;
        }
        else if (animator.GetBool("IsUppercut"))
        {
            offset = facingRight ? uppercutRightOffset : uppercutLeftOffset;
        }
        else if (animator.GetBool("IsHeavyPunch"))
        {
            offset = facingRight ? heavyPunchRightOffset : heavyPunchLeftOffset;
        }
        else if (animator.GetBool("IsShoot"))
        {
            offset = facingRight ? shootRightOffset : shootLeftOffset;
        }
        else
        {
            offset = facingRight ? idleRightOffset : idleLeftOffset;
        }

        targetGameObject.localPosition = defaultLocalPosition + offset;
    }
}
