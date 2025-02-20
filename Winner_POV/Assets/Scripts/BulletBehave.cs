using UnityEngine;
using System.Collections;

public class BulletBehave : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float fadeDelay = 1f;     // How long to wait before starting fade
    public float fadeDuration = 1f;   // How long the fade takes
    public LayerMask enemyLayer;      // Layer for enemies

    private float timeAlive = 0f;
    private SpriteRenderer spriteRenderer;
    private bool isFading = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on bullet!");
            return;
        }
    }

    void Update()
    {
        timeAlive += Time.deltaTime;

        if (timeAlive >= fadeDelay && !isFading)
        {
            StartFading();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Instantly destroy if hitting enemy
        if ((enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            StopAllCoroutines(); // Stop any active fade
            Destroy(gameObject);
        }
    }

    void StartFading()
    {
        if (spriteRenderer != null && !isFading)
        {
            isFading = true;
            StartCoroutine(FadeCoroutine());
        }
    }

    IEnumerator FadeCoroutine()
    {
        Color startColor = spriteRenderer.color;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
