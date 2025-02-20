using UnityEngine;

public class EnemyStun : MonoBehaviour
{
    private bool isStunned = false;
    private float stunEndTime;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isStunned && Time.time >= stunEndTime)
        {
            isStunned = false;
            rb.linearVelocity = Vector2.zero; // Stop movement after stun
            // Optionally: Change the enemy's state back to normal here
        }
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;

        // Optionally: Add a visual or gameplay effect (like a particle or animation)
        Debug.Log(name + " is stunned for " + duration + " seconds!");
    }

    void FixedUpdate()
    {
        if (isStunned)
        {
            rb.linearVelocity = Vector2.zero; // Prevent enemy from moving while stunned
        }
    }
}
