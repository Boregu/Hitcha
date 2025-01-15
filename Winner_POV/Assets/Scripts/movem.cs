using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movem : MonoBehaviour
{
    Rigidbody2D rb;
    public float hspeed = 5;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector2(Input.GetAxisRaw("Horizontal") * hspeed, 0);
    }
}
