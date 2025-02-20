using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PolygonCollider2D), typeof(SpriteRenderer))]
public class DynamicPixelPerfectCollider : MonoBehaviour {
    private PolygonCollider2D polygonCollider;
    private SpriteRenderer spriteRenderer;
    private Sprite lastSprite; 

    private void Awake() {
        polygonCollider = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate() {
        // Only update if the sprite has changed
        if (spriteRenderer.sprite != lastSprite) {
            GenerateCollider();
            lastSprite = spriteRenderer.sprite;
        }
    }

    private void GenerateCollider() {
        if (spriteRenderer.sprite == null) return;

        Texture2D texture = spriteRenderer.sprite.texture;
        polygonCollider.pathCount = 0; // Clear previous shape

        List<Vector2> newPath = GetAlphaShape(texture, spriteRenderer.sprite);

        if (newPath.Count > 2) {
            polygonCollider.SetPath(0, newPath.ToArray());
        }
    }

    private List<Vector2> GetAlphaShape(Texture2D texture, Sprite sprite) {
        List<Vector2> shape = new List<Vector2>();

        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;

        float pixelsPerUnit = sprite.pixelsPerUnit;
        Vector2 spriteOffset = sprite.bounds.center;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (pixels[y * width + x].a > 0.1f) { // If pixel is not fully transparent
                    Vector2 worldPos = new Vector2(
                        (x - width / 2f) / pixelsPerUnit + spriteOffset.x,
                        (y - height / 2f) / pixelsPerUnit + spriteOffset.y
                    );

                    shape.Add(worldPos);
                }
            }
        }

        return shape;
    }
}
