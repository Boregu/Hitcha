using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class senemymovement : MonoBehaviour
{
    public float speed;
    private GameObject player;
    public bool chase = false;
    public Transform startingpoint;
    public float distanceFromPlayer;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            return;
        if (chase == true)
            Chase();
        else
            ReturnStartPoint();

        Flip();
    }

    private void Chase()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, player.transform.position) <= distanceFromPlayer)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, -speed * Time.deltaTime);
        }
    }

    private void ReturnStartPoint()
    {
        transform.position = Vector2.MoveTowards(transform.position, startingpoint.position, speed * Time.deltaTime);
    }

    private void Flip()
    {
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
