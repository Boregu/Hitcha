using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 10f; // Speed of the projectile
    public float lifetime = 5f; // Time before the projectile disappears

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); // Destroy the projectile after a set time
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Destroy(gameObject); // Destroy the projectile if it hits the player
        }
        else if (collision.collider.CompareTag("Wall"))
        {
            Destroy(gameObject); // Destroy projectile if it hits a wall
        }
    }
}
