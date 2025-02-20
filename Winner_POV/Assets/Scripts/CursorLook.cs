using UnityEngine;

public class CursorLook : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private float smoothSpeed = 0.3f; // Adjust this to control how smoothly the object follows the cursor
    private Vector3 targetPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Make sure you have a camera tagged as 'MainCamera' in the scene.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera == null) return;

        // Get mouse position and convert it to world coordinates
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = transform.position.z - mainCamera.transform.position.z;
        targetPosition = mainCamera.ScreenToWorldPoint(mousePos);
        
        // Smoothly move the object towards the cursor
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
