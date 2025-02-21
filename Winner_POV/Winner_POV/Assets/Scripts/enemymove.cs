using UnityEngine;

public class fenemymove : MonoBehaviour
{
    public float speed = 2f; // Overall movement speed
    public float aggroRange = 10f; // Aggro range for the enemy to start chasing
    public float chaseDelay = 1f; // Delay before chasing starts again
    public float xOffset = 1.5f; // Distance offset on the X-axis from the player

    private GameObject player; // Reference to the player
    private bool isChasing = false; // Whether the enemy is chasing
    private bool chaseOverridden = false; // If chase state is overridden
    private Vector2 lastPlayerPosition; // Track player's last known position
    private float lastPlayerMoveTime; // Timestamp of the last significant player movement
    public Transform startingpoint; // Starting position of the enemy

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            lastPlayerPosition = player.transform.position;
    }

    void Update()
    {
        if (player == null)
            return;

        if (!chaseOverridden)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

            // Check if the player's position has changed significantly
            if (Vector2.Distance(player.transform.position, lastPlayerPosition) > 0.1f)
            {
                lastPlayerMoveTime = Time.time; // Reset the delay timer
                lastPlayerPosition = player.transform.position;
            }

            // Enable chase if player is within aggro range and delay has passed
            isChasing = distanceToPlayer <= aggroRange && Time.time >= lastPlayerMoveTime + chaseDelay;
        }

        if (isChasing)
            Chase();
        else
            ReturnStartPoint();

        Flip();
    }

    public void SetChase(bool value)
    {
        chaseOverridden = true;
        isChasing = value;

        // Reset override after a frame if set to false
        if (!value)
            chaseOverridden = false;
    }

    private void Chase()
    {
        float deltaTime = Time.deltaTime;

        // Calculate target position with an X-axis offset from the player
        Vector2 targetPosition = player.transform.position;
        targetPosition.x += (player.transform.position.x > transform.position.x ? -xOffset : xOffset);

        // Smooth movement towards the target position
        Vector2 newPosition = Vector2.MoveTowards(transform.position, targetPosition, speed * deltaTime);
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }

    private void ReturnStartPoint()
    {
        // Smooth movement back to the starting position
        Vector2 newPosition = Vector2.MoveTowards(transform.position, startingpoint.position, speed * Time.deltaTime);
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }

    private void Flip()
    {
        // Flip sprite to face the player
        if (player == null) return;

        if (transform.position.x > player.transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
}
