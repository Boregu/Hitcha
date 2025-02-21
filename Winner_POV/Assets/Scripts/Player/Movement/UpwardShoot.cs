using UnityEngine;
using Cinemachine; // Add Cinemachine namespace
using System.Collections.Generic;

public class RotateUpperBodyTowardMouse : MonoBehaviour
{
    [Header("References")]
    public Transform upperBody;
    public Transform secondaryAimObject; // Secondary aim object
    public Camera mainCamera;
    public AnimationHandler animationHandler;
    public Transform targetGameObject;
    public Transform aimReferencePoint;
    public CircleCollider2D aimConstraintCollider;
    public CinemachineVirtualCamera virtualCamera; // Reference to virtual camera
    public Collider2D leftSideCollider;  // Add reference to left side trigger
    public Collider2D rightSideCollider; // Add reference to right side trigger
    public GameObject flipScaleObject; // New reference for the object to flip scale

    [Header("Secondary Aim Settings")]
    public bool useSecondaryAim = true;
    public Vector3 secondaryAimOffset = Vector3.zero;
    public float secondaryAimRotationOffset = 0f;

    [Header("Offsets for Animations (Right)")]
    public Vector3 idleRightOffset = Vector3.zero;
    public float idleRightRotationZ = 0f;
    public float idleRightScaleX = 1f;

    public Vector3 shootRightOffset = Vector3.zero;
    public float shootRightRotationZ = 0f;
    public float shootRightScaleX = 1f;

    public Vector3 punchRightOffset = Vector3.zero;
    public float punchRightRotationZ = 0f;
    public float punchRightScaleX = 1f;

    public Vector3 uppercutRightOffset = Vector3.zero;
    public float uppercutRightRotationZ = 0f;
    public float uppercutRightScaleX = 1f;

    public Vector3 heavyPunchRightOffset = Vector3.zero;
    public float heavyPunchRightRotationZ = 0f;
    public float heavyPunchRightScaleX = 1f;

    public Vector3 chargingHeavyPunchRightOffset = Vector3.zero;
    public float chargingHeavyPunchRightRotationZ = 0f;
    public float chargingHeavyPunchRightScaleX = 1f;

    [Header("Offsets for Animations (Left)")]
    public Vector3 idleLeftOffset = Vector3.zero;
    public float idleLeftRotationZ = 0f;
    public float idleLeftScaleX = 1f;

    public Vector3 shootLeftOffset = Vector3.zero;
    public float shootLeftRotationZ = 0f;
    public float shootLeftScaleX = 1f;

    public Vector3 punchLeftOffset = Vector3.zero;
    public float punchLeftRotationZ = 0f;
    public float punchLeftScaleX = 1f;

    public Vector3 uppercutLeftOffset = Vector3.zero;
    public float uppercutLeftRotationZ = 0f;
    public float uppercutLeftScaleX = 1f;

    public Vector3 heavyPunchLeftOffset = Vector3.zero;
    public float heavyPunchLeftRotationZ = 0f;
    public float heavyPunchLeftScaleX = 1f;

    public Vector3 chargingHeavyPunchLeftOffset = Vector3.zero;
    public float chargingHeavyPunchLeftRotationZ = 0f;
    public float chargingHeavyPunchLeftScaleX = 1f;

    [Header("Aim Settings")]
    public float aimOffsetAngle = 0f;
    public float horizontalThreshold = 0.1f;
    public float minimumAimRadius = 2f;

    [System.Serializable]
    public enum DisableType
    {
        SpriteRendererOnly,
        EntireGameObject
    }

    [System.Serializable]
    public struct DisableItem
    {
        public SpriteRenderer spriteRenderer;
        public DisableType disableType;
    }

    [System.Serializable]
    public class AnimationDisableControl
    {
        public DisableItem[] itemsToDisable;
    }

    [Header("Animation GameObject Controls")]
    public AnimationDisableControl idleDisables;
    public AnimationDisableControl shootDisables;
    public AnimationDisableControl punchDisables;
    public AnimationDisableControl uppercutDisables;
    public AnimationDisableControl heavyPunchDisables;
    public AnimationDisableControl chargingHeavyPunchDisables;

    private Vector3 defaultLocalPosition;
    private Vector3 secondaryDefaultLocalPosition;
    [SerializeField] public bool IsFacingRight;
    private PlayerAttack playerAttack;
    private Vector3 currentOffset;
    private Vector3 targetOffset;

    private Dictionary<Transform, float> originalChildYPositions = new Dictionary<Transform, float>();
    private Dictionary<Transform, float> secondaryChildYPositions = new Dictionary<Transform, float>();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (upperBody == null) Debug.LogError("UpperBody Transform not assigned!");
        if (targetGameObject == null) Debug.LogError("Target GameObject not assigned!");
        if (animationHandler == null) Debug.LogError("AnimationHandler not assigned!");

        // Store original Y positions of children
        if (targetGameObject != null)
        {
            foreach (Transform child in targetGameObject)
            {
                originalChildYPositions[child] = child.localPosition.y;
            }
        }

        if (secondaryAimObject != null)
        {
            foreach (Transform child in secondaryAimObject)
            {
                secondaryChildYPositions[child] = child.localPosition.y;
            }
        }

        defaultLocalPosition = targetGameObject.localPosition;
        if (secondaryAimObject != null)
        {
            secondaryDefaultLocalPosition = secondaryAimObject.localPosition;
        }
        playerAttack = GetComponent<PlayerAttack>();
        currentOffset = Vector3.zero;
        targetOffset = Vector3.zero;
    }

    void UpdateChildPositions(Transform parent, Dictionary<Transform, float> originalPositions, bool isFacingRight)
    {
        if (parent != null)
        {
            foreach (Transform child in parent)
            {
                if (originalPositions.ContainsKey(child))
                {
                    Vector3 childPos = child.localPosition;
                    childPos.y = isFacingRight ? originalPositions[child] : -originalPositions[child];
                    child.localPosition = childPos;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null || upperBody == null || targetGameObject == null || animationHandler == null || aimReferencePoint == null) return;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Store the original aim reference point position
        Vector3 originalAimPosition = aimReferencePoint.position;

        // Calculate aim direction before applying any offsets
        Vector3 directionToMouse = worldMousePosition - originalAimPosition;
        Vector3 direction = directionToMouse.normalized;
        
        if (virtualCamera != null)
        {
            direction = virtualCamera.transform.InverseTransformDirection(direction);
        }

        // Handle direction switching
        Vector2 mousePos2D = new Vector2(worldMousePosition.x, worldMousePosition.y);
        bool isInRightCollider = rightSideCollider.OverlapPoint(mousePos2D);
        bool isInLeftCollider = leftSideCollider.OverlapPoint(mousePos2D);

        // Debug visualization
        Debug.DrawLine(transform.position, worldMousePosition, Color.yellow);

        // Update facing direction based on colliders
        if (!animationHandler.IsPunching)
        {
            if (isInRightCollider && !isInLeftCollider)
            {
                IsFacingRight = true;
            }
            else if (isInLeftCollider && !isInRightCollider)
            {
                IsFacingRight = false;
            }
        }
        else
        {
            // For punching, use horizontal direction
            IsFacingRight = transform.position.x < worldMousePosition.x;
        }

        // Update child positions based on facing direction
        UpdateChildPositions(targetGameObject, originalChildYPositions, IsFacingRight);

        // Update flip scale object
        if (flipScaleObject != null)
        {
            Vector3 scale = flipScaleObject.transform.localScale;
            scale.x = 1f;
            flipScaleObject.transform.localScale = scale;
        }

        // Calculate rotation angle based on the original aim position
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + aimOffsetAngle;
        float baseAngle = IsFacingRight ? angle : angle + 180f;

        // Apply scale and rotation to main object
        upperBody.localScale = new Vector3(IsFacingRight ? 1 : -1, 1, 1);
        
        // Apply animation offset after calculating aim
        ApplyAnimationOffset(baseAngle);

        // Handle secondary aim object
        if (useSecondaryAim && secondaryAimObject != null)
        {
            // Apply rotation to secondary object using the original aim calculation
            float secondaryAngle = angle + secondaryAimRotationOffset;
            secondaryAimObject.rotation = Quaternion.Euler(0, 0, secondaryAngle);
            
            // Apply position offset and scale
            secondaryAimObject.localPosition = secondaryDefaultLocalPosition + 
                (IsFacingRight ? secondaryAimOffset : new Vector3(-secondaryAimOffset.x, secondaryAimOffset.y, secondaryAimOffset.z));
            
            // Keep scale always at 1
            Vector3 secondaryScale = secondaryAimObject.localScale;
            secondaryScale.x = 1f;
            secondaryAimObject.localScale = secondaryScale;

            // Update secondary object's child positions
            UpdateChildPositions(secondaryAimObject, secondaryChildYPositions, IsFacingRight);
        }
    }

    void ApplyAnimationOffset(float baseRotation)
    {
        float targetRotationZ = 0f;
        float targetScaleX = 1f;
        AnimationDisableControl currentDisables = null;
        float finalRotation = baseRotation; // Store the base rotation

        // Check states in priority order and directly set the offset
        if (playerAttack != null && playerAttack.IsChargingHeavyPunch)
        {
            targetOffset = IsFacingRight ? chargingHeavyPunchRightOffset : chargingHeavyPunchLeftOffset;
            targetRotationZ = IsFacingRight ? chargingHeavyPunchRightRotationZ : chargingHeavyPunchLeftRotationZ;
            targetScaleX = IsFacingRight ? chargingHeavyPunchRightScaleX : chargingHeavyPunchLeftScaleX;
            currentDisables = chargingHeavyPunchDisables;
        }
        else if (animationHandler.IsHeavyPunching)
        {
            targetOffset = IsFacingRight ? heavyPunchRightOffset : heavyPunchLeftOffset;
            targetRotationZ = IsFacingRight ? heavyPunchRightRotationZ : heavyPunchLeftRotationZ;
            targetScaleX = IsFacingRight ? heavyPunchRightScaleX : heavyPunchLeftScaleX;
            currentDisables = heavyPunchDisables;
            // Override rotation for heavy punch
            finalRotation = IsFacingRight ? 0 : 180;
        }
        else if (animationHandler.IsUppercutting)
        {
            targetOffset = IsFacingRight ? uppercutRightOffset : uppercutLeftOffset;
            targetRotationZ = IsFacingRight ? uppercutRightRotationZ : uppercutLeftRotationZ;
            targetScaleX = IsFacingRight ? uppercutRightScaleX : uppercutLeftScaleX;
            currentDisables = uppercutDisables;
            // Override rotation for uppercut
            finalRotation = IsFacingRight ? 0 : 180;
        }
        else if (animationHandler.IsPunching)
        {
            targetOffset = IsFacingRight ? punchRightOffset : punchLeftOffset;
            targetRotationZ = IsFacingRight ? punchRightRotationZ : punchLeftRotationZ;
            targetScaleX = IsFacingRight ? punchRightScaleX : punchLeftScaleX;
            currentDisables = punchDisables;
            // Override rotation for punch
            finalRotation = IsFacingRight ? 0 : 180;
        }
        else if (animationHandler.IsShooting)
        {
            targetOffset = IsFacingRight ? shootRightOffset : shootLeftOffset;
            targetRotationZ = IsFacingRight ? shootRightRotationZ : shootLeftRotationZ;
            targetScaleX = IsFacingRight ? shootRightScaleX : shootLeftScaleX;
            currentDisables = shootDisables;
        }
        else // Idle state
        {
            targetOffset = IsFacingRight ? idleRightOffset : idleLeftOffset;
            targetRotationZ = IsFacingRight ? idleRightRotationZ : idleLeftRotationZ;
            targetScaleX = IsFacingRight ? idleRightScaleX : idleLeftScaleX;
            currentDisables = idleDisables;
        }

        // Apply position offset
        targetGameObject.localPosition = defaultLocalPosition + targetOffset;
        
        // Apply combined rotation (using finalRotation instead of baseRotation for melee attacks)
        upperBody.rotation = Quaternion.Euler(0, 0, finalRotation + targetRotationZ);
        
        // Apply scale (maintaining Y and Z scale)
        Vector3 currentScale = targetGameObject.localScale;
        targetGameObject.localScale = new Vector3(targetScaleX * (IsFacingRight ? 1 : -1), currentScale.y, currentScale.z);

        // Handle disabling objects
        HandleDisableControls(currentDisables);
    }

    private void HandleDisableControls(AnimationDisableControl currentState)
    {
        // First, enable all sprite renderers
        EnableAllItems();

        // Then disable the items for the current state
        if (currentState != null && currentState.itemsToDisable != null)
        {
            foreach (DisableItem item in currentState.itemsToDisable)
            {
                if (item.spriteRenderer != null)
                {
                    if (item.disableType == DisableType.EntireGameObject)
                    {
                        // If we're disabling the entire GameObject, disable it
                        item.spriteRenderer.gameObject.SetActive(false);
                    }
                    else
                    {
                        // If we're just disabling the SpriteRenderer, disable only that
                        item.spriteRenderer.enabled = false;
                    }
                }
            }
        }
    }

    private void EnableAllItems()
    {
        // Create array of all possible states
        AnimationDisableControl[] allStates = new AnimationDisableControl[] 
        {
            idleDisables,
            shootDisables,
            punchDisables,
            uppercutDisables,
            heavyPunchDisables,
            chargingHeavyPunchDisables
        };

        // Enable all items from all states
        foreach (var state in allStates)
        {
            if (state != null && state.itemsToDisable != null)
            {
                foreach (DisableItem item in state.itemsToDisable)
                {
                    if (item.spriteRenderer != null)
                    {
                        // Always enable the GameObject
                        item.spriteRenderer.gameObject.SetActive(true);
                        
                        // Enable the SpriteRenderer
                        item.spriteRenderer.enabled = true;
                    }
                }
            }
        }
    }

    public void ResetToDefaultPosition()
    {
        if (targetGameObject != null)
        {
            targetGameObject.localPosition = defaultLocalPosition;
        }
        if (secondaryAimObject != null)
        {
            secondaryAimObject.localPosition = secondaryDefaultLocalPosition;
        }
    }
}