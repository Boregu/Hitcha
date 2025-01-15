using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemynpcpatrol : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;
    private Rigidbody2D rb;
    private Transform currentPoint;
    public float speed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPoint = pointB.transform; // Start moving towards pointB
    }

    void Update()
    {
        // Calculate direction to the current point
        Vector2 direction = (currentPoint.position - transform.position).normalized;

        // Move the Rigidbody in the direction
        rb.linearVelocity = direction * speed;

        // Check if close enough to switch target points
        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f)
        {
            // Switch target point
            currentPoint = currentPoint == pointB.transform ? pointA.transform : pointB.transform;
        }


    }
}
