using UnityEngine;

public class RotateUpperBodyTowardMouse : MonoBehaviour
{
    public Transform upperBody; // Reference to the UpperBody GameObject
    public Camera mainCamera; // Reference to the Main Camera

    public GameObject rightFacingSprite; // Reference to the right-facing sprite
    public GameObject leftFacingSprite; // Reference to the left-facing sprite

    private bora movementScript; // Reference to the movement script on the parent

    void Start()
    {
        // Find the movement script on the parent GameObject
        movementScript = GetComponentInParent<bora>();

        if (movementScript == null)
        {
            Debug.LogError("Movement script (bora) not found on the parent GameObject!");
        }
    }

    void Update()
    {
        // If the movement script is not assigned, don't proceed
        if (movementScript == null) return;

        // Check movement states to determine sprite visibility
        if (movementScript.IsWallSliding || movementScript.IsDashing)
        {
            // Hide both sprites when wall sliding or dashing
            rightFacingSprite.SetActive(false);
            leftFacingSprite.SetActive(false);
            return; // Skip rotation logic
        }

        // Ensure sprites are visible if not wall sliding or dashing
        rightFacingSprite.SetActive(true);
        leftFacingSprite.SetActive(true);

        // Step 1: Get the mouse position in world space
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Step 2: Calculate the direction from the upper body to the mouse position
        Vector3 direction = mousePosition - upperBody.position;

        // Step 3: Calculate the rotation angle in 2D space (Z-axis rotation)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Step 4: Flip only the upper body when moving left or right
        bool facingRight = direction.x > 0;

        if (facingRight)
        {
            // Activate the right-facing sprite and set its rotation
            rightFacingSprite.SetActive(true);
            leftFacingSprite.SetActive(false);

            // Rotate the right-facing sprite toward the mouse
            rightFacingSprite.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // Activate the left-facing sprite and set its rotation
            rightFacingSprite.SetActive(false);
            leftFacingSprite.SetActive(true);

            // Rotate the left-facing sprite toward the mouse
            // Adjust the angle to account for the flipped orientation
            leftFacingSprite.transform.rotation = Quaternion.Euler(0, 0, angle - 180f);
        }
    }
}
