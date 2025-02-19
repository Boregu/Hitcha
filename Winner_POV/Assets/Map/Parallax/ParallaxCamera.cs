using UnityEngine;
using Cinemachine;

[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private Camera mainCamera;
    private Vector3 previousCameraPosition;

    void Start()
    {
        mainCamera = Camera.main;
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        previousCameraPosition = mainCamera.transform.position;
    }

    public float GetCameraMovement()
    {
        if (mainCamera == null) return 0f;
        
        float delta = previousCameraPosition.x - mainCamera.transform.position.x;
        previousCameraPosition = mainCamera.transform.position;
        return delta;
    }
}