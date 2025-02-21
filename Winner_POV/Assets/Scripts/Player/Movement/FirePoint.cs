using UnityEngine;

public class AdjustFirePoint : MonoBehaviour
{
    public Transform firePoint; // Reference to the fire point
    public Camera mainCamera; // Reference to the main camera

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (firePoint == null || mainCamera == null)
            return;

        // Get the mouse position in world space
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z; // Ensure the z position is set to the camera's depth
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Calculate the direction from the firePoint to the mouse position
        Vector3 direction = (worldMousePosition - firePoint.position).normalized;

        // Rotate the firePoint to face the mouse position
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }
}
