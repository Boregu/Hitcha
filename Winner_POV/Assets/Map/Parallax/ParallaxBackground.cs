using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private ParallaxCamera parallaxCamera;
    private List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();

    void Start()
    {
        if (parallaxCamera == null)
        {
            parallaxCamera = Camera.main.GetComponent<ParallaxCamera>();
            if (parallaxCamera == null)
            {
                parallaxCamera = Camera.main.gameObject.AddComponent<ParallaxCamera>();
            }
        }
        SetLayers();
    }

    void LateUpdate()
    {
        if (parallaxCamera != null)
        {
            float deltaMovement = parallaxCamera.GetCameraMovement();
            if (deltaMovement != 0)
            {
                foreach (ParallaxLayer layer in parallaxLayers)
                {
                    layer.Move(deltaMovement);
                }
            }
        }
    }

    void SetLayers()
    {
        parallaxLayers.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();
            if (layer != null)
            {
                layer.name = "Layer-" + i;
                parallaxLayers.Add(layer);
            }
        }
    }
}