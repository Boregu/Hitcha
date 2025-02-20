using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Visual Effects")]
    public GameObject muzzleFlashLight;
    public float flashLightDuration = 0.05f;
    public GameObject muzzleFlashParticlePrefab;
    public GameObject hitEffectPrefab;
    public Transform firePoint;
    
    private float flashLightEndTime;

    void Start()
    {
        if (muzzleFlashLight != null)
            muzzleFlashLight.SetActive(false);
    }

    void Update()
    {
        if (muzzleFlashLight != null && muzzleFlashLight.activeSelf && Time.time >= flashLightEndTime)
            muzzleFlashLight.SetActive(false);
    }

    public void PlayShootEffects()
    {
        // Handle muzzle flash light
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.SetActive(true);
            flashLightEndTime = Time.time + flashLightDuration;
        }

        // Handle muzzle flash particle effect
        if (muzzleFlashParticlePrefab != null && firePoint != null)
        {
            GameObject muzzleFlash = Instantiate(muzzleFlashParticlePrefab, firePoint.position, firePoint.rotation);
            Destroy(muzzleFlash, 2f);
        }
    }

    public void PlayHitEffect(Vector3 hitPosition, Quaternion hitRotation)
    {
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, hitPosition, hitRotation);
            Destroy(hitEffect, 2f);
        }
    }
}