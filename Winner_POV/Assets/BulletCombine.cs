using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

public class BulletCombine : MonoBehaviour
{
    [Header("Combining Settings")]
    public float scaleIncreasePerBullet = 0.25f;
    public float explosionCountdown = 3f;
    public float baseScale = 1f;

    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 10f;
    public float explosionDamage = 50f;
    public LayerMask enemyLayers;

    [Header("Light Settings")]
    public Light2D innerExplosionLight;
    public Light2D outerExplosionLight;
    public float maxLightIntensity = 3f;
    public float explosionLightDuration = 0.5f;
    public float lightPulseSpeed = 10f;

    private List<GameObject> combinedBullets = new List<GameObject>();
    private bool isExploding = false;
    private float currentCountdown;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.linearDamping = 1f;
        }

        // Create lights if they don't exist
        if (innerExplosionLight == null)
        {
            GameObject innerLight = new GameObject("InnerExplosionLight");
            innerLight.transform.parent = transform;
            innerLight.transform.localPosition = Vector3.zero;
            innerExplosionLight = innerLight.AddComponent<Light2D>();
            innerExplosionLight.intensity = 0;
            innerExplosionLight.color = Color.red;
            innerExplosionLight.pointLightOuterRadius = 3f;
        }

        if (outerExplosionLight == null)
        {
            GameObject outerLight = new GameObject("OuterExplosionLight");
            outerLight.transform.parent = transform;
            outerLight.transform.localPosition = Vector3.zero;
            outerExplosionLight = outerLight.AddComponent<Light2D>();
            outerExplosionLight.intensity = 0;
            outerExplosionLight.color = new Color(1f, 0.5f, 0f); // Orange
            outerExplosionLight.pointLightOuterRadius = 5f;
        }

        // Add this bullet to the combined list
        combinedBullets.Add(gameObject);
        UpdateScale();
        StartCountdown();
    }

    // Update is called once per frame
    void Update()
    {
        if (isExploding) return;

        currentCountdown -= Time.deltaTime;
        if (currentCountdown <= 0)
        {
            StartCoroutine(Explode());
        }

        // Optional: Add visual feedback for countdown
        float pulseIntensity = Mathf.PingPong(Time.time * lightPulseSpeed, 1f) * (1f - (currentCountdown / explosionCountdown));
        if (innerExplosionLight != null)
            innerExplosionLight.intensity = pulseIntensity;
        if (outerExplosionLight != null)
            outerExplosionLight.intensity = pulseIntensity * 0.5f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isExploding) return;

        BulletBehave otherBullet = other.GetComponent<BulletBehave>();
        if (otherBullet != null)
        {
            // If the other bullet is not part of a combo, combine it
            BulletCombine otherCombiner = other.GetComponent<BulletCombine>();
            if (otherCombiner == null)
            {
                CombineWithBullet(other.gameObject);
            }
        }
    }

    void CombineWithBullet(GameObject bullet)
    {
        // Disable the other bullet's behavior and collider
        BulletBehave bulletBehavior = bullet.GetComponent<BulletBehave>();
        if (bulletBehavior != null)
        {
            bulletBehavior.enabled = false;
        }

        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        if (bulletCollider != null)
        {
            bulletCollider.enabled = false;
        }

        // Parent the bullet to this object and reset its local position
        bullet.transform.parent = transform;
        bullet.transform.localPosition = Vector3.zero;

        // Add to the list and update scale
        combinedBullets.Add(bullet);
        UpdateScale();

        // Reset countdown when new bullet is added
        StartCountdown();
    }

    void UpdateScale()
    {
        float newScale = baseScale + (scaleIncreasePerBullet * (combinedBullets.Count - 1));
        transform.localScale = new Vector3(newScale, newScale, 1f);

        // Update light radiuses based on scale
        if (innerExplosionLight != null)
            innerExplosionLight.pointLightOuterRadius = 3f * newScale;
        if (outerExplosionLight != null)
            outerExplosionLight.pointLightOuterRadius = 5f * newScale;
    }

    void StartCountdown()
    {
        currentCountdown = explosionCountdown;
    }

    IEnumerator Explode()
    {
        isExploding = true;

        // Explosion light effect
        float elapsedTime = 0f;
        while (elapsedTime < explosionLightDuration)
        {
            float intensity = Mathf.Lerp(maxLightIntensity, 0f, elapsedTime / explosionLightDuration);
            
            if (innerExplosionLight != null)
                innerExplosionLight.intensity = intensity;
            if (outerExplosionLight != null)
                outerExplosionLight.intensity = intensity * 0.8f;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Apply explosion force and damage
        float explosionRadiusScaled = explosionRadius * transform.localScale.x;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadiusScaled, enemyLayers);
        
        foreach (Collider2D hit in hitColliders)
        {
            Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();
            if (hitRb != null)
            {
                Vector2 direction = (hit.transform.position - transform.position).normalized;
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                float forceMagnitude = Mathf.Lerp(explosionForce, 0f, distance / explosionRadiusScaled);
                hitRb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);
            }
        }

        // Destroy all combined bullets
        foreach (GameObject bullet in combinedBullets)
        {
            Destroy(bullet);
        }

        // Destroy this object
        Destroy(gameObject);
    }
}
