using UnityEngine;

public class SlopeSpeedBoost : MonoBehaviour
{
    [Header("Slope Settings")]
    public LayerMask slopeLayer;         // Layer for slopes
    public float maxFallSpeedBoost = 15f; // Maximum speed boost when landing near the slope's edge
    public float edgeDistanceThreshold = 2f; // Distance from the edge for maximum boost (adjust this!)

    [Header("Debugging")]
    public bool enableDebugging = true;   // Toggle debugging visuals/logs
    public Color slopeDebugColor = Color.green; // Color for slope debug lines
    public Color edgeDebugColor = Color.blue;   // Color for edge debug lines

    private Rigidbody2D rb;             // Player's Rigidbody2D
    private bool isOnSlope = false;     // Is the player currently on a slope?
    private Collider2D currentSlope;    // The collider of the slope we’re standing on

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we collided with a slope
        if (IsInLayerMask(collision.gameObject.layer, slopeLayer))
        {
            isOnSlope = true;
            currentSlope = collision.collider;

            if (enableDebugging)
            {
                Debug.Log("Standing on a slope!");
            }

            // Calculate the edge-boost mechanic
            CalculateSlopeBoost(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Exit slope detection
        if (collision.collider == currentSlope)
        {
            isOnSlope = false;
            currentSlope = null;

            if (enableDebugging)
            {
                Debug.Log("Left the slope.");
            }
        }
    }

    private void FixedUpdate()
    {
        if (isOnSlope)
        {
            AdjustVelocityOnSlope();
        }
    }

    private void AdjustVelocityOnSlope()
    {
        // Determine the direction of movement
        float moveDirection = Input.GetAxis("Horizontal");

        // Only adjust velocity if the player is moving
        if (Mathf.Abs(moveDirection) > 0.1f)
        {
            // Calculate the slope's angle
            Vector2 slopeNormal = currentSlope.transform.up;

            // Calculate the desired velocity along the slope
            Vector2 desiredVelocity = new Vector2(moveDirection * rb.linearVelocity.magnitude, rb.linearVelocity.y);

            // Project the desired velocity onto the slope
            Vector2 slopeParallel = Vector2.Perpendicular(slopeNormal);
            Vector2 slopeVelocity = Vector2.Dot(desiredVelocity, slopeParallel) * slopeParallel;

            // Apply the velocity
            rb.linearVelocity = slopeVelocity;
        }

        // Apply a small downward force to keep the player grounded only if not jumping
        if (rb.linearVelocity.y <= 0)
        {
            rb.AddForce(Vector2.down * 10f);
        }
    }

    private void CalculateSlopeBoost(Collision2D collision)
    {
        // Get the slope's bounds
        Bounds slopeBounds = collision.collider.bounds;

        // Calculate player's horizontal position relative to the slope
        float playerX = transform.position.x;
        float slopeLeftEdge = slopeBounds.min.x;
        float slopeRightEdge = slopeBounds.max.x;

        // Debug: Draw slope bounds
        if (enableDebugging)
        {
            Debug.DrawLine(new Vector3(slopeLeftEdge, slopeBounds.min.y, 0), new Vector3(slopeRightEdge, slopeBounds.min.y, 0), slopeDebugColor, 0.5f);
        }

        // Determine how close the player is to the bottom-right edge
        float distanceToEdge = Mathf.Abs(playerX - slopeRightEdge);
        float distanceFactor = Mathf.Clamp01(1f - (distanceToEdge / edgeDistanceThreshold)); // Closer to 0 = near edge

        // Debug: Log distance values
        if (enableDebugging)
        {
            Debug.Log($"Player X: {playerX}, Slope Right Edge: {slopeRightEdge}, Distance to Edge: {distanceToEdge}, Distance Factor: {distanceFactor}");
        }

        // Ensure fall speed is sufficient for boost
        float fallSpeed = Mathf.Abs(rb.linearVelocity.y); // Use player's vertical fall speed
        if (fallSpeed <= 0.1f) // Small threshold to avoid applying boost when not falling
        {
            if (enableDebugging)
            {
                Debug.Log("Not enough fall speed to apply a boost.");
            }
            return;
        }

        // Calculate the speed boost
        float fallSpeedBoost = Mathf.Clamp(fallSpeed * distanceFactor, 0, maxFallSpeedBoost);

        // Debug: Log final speed boost
        if (enableDebugging)
        {
            Debug.Log($"Calculated Fall Speed: {fallSpeed}, Applied Speed Boost: {fallSpeedBoost}");
        }

        // Boost player speed in the slope's direction
        Vector2 slopeDirection = Vector2.right; // Assume slopes are slanted to the right by default
        if (slopeLeftEdge > slopeRightEdge) slopeDirection = Vector2.left; // Adjust direction if the slope faces left

        rb.linearVelocity = new Vector2(slopeDirection.x * fallSpeedBoost, rb.linearVelocity.y);

        // Debug: Log slope direction and applied velocity
        if (enableDebugging)
        {
            Debug.Log($"Boost Applied in Direction {slopeDirection}, Final Velocity: {rb.linearVelocity}");
        }
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << layer)) > 0);
    }
}