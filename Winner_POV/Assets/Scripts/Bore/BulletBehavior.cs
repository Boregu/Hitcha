using UnityEngine;
using System.Collections;

public class BulletBehavior : MonoBehaviour
{
    public float fadeDelay = 1f;     // How long to wait before starting fade
    public float fadeDuration = 1f;  // How long the fade takes
    public LayerMask enemyLayer;     // Layer for enemies

    private SpriteRenderer spriteRenderer;
    private bool isFading = false;
    private float fadeStartTime;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we hit an enemy
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            // Instant destroy on enemy hit
            Destroy(gameObject);
            return;
        }

        // If we hit anything else, start the fade sequence
        if (!isFading)
        {
            StartCoroutine(FadeOutSequence());
        }
    }

    IEnumerator FadeOutSequence()
    {
        isFading = true;

        // Wait for fade delay
        yield return new WaitForSeconds(fadeDelay);

        fadeStartTime = Time.time;
        
        // Gradually fade out
        while (Time.time - fadeStartTime < fadeDuration)
        {
            float alpha = 1f - ((Time.time - fadeStartTime) / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Ensure we're fully transparent before destroying
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        Destroy(gameObject);
    }
} 