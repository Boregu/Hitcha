using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] private GameObject lineObject;
    [SerializeField] private float lineSpacing = 1f;
    [SerializeField] int maxDots = 25;

    Transform parent;
    public void DrawLine(Vector2 pos1, Vector2 pos2)
    {
        transform.position = pos1;
        if (pos2.x - pos1.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 0, Vector2.Angle(Vector2.up, pos2 - pos1));
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, -Vector2.Angle(Vector2.up, pos2 - pos1));
        }

        float distance = Mathf.Sqrt((pos2.x - pos1.x) * (pos2.x - pos1.x) + (pos2.y - pos1.y) * (pos2.y - pos1.y));
        float elapsedDistance = 0f;

        while (distance > elapsedDistance || maxDots < 1)
        {
            transform.Translate(0, lineSpacing, 0);
            elapsedDistance += lineSpacing;

            parent = GameObject.Find("map").GetComponent<Transform>();
            Instantiate(lineObject, transform.position, transform.rotation, parent);

            maxDots--;
        }
        Destroy(gameObject);
    }
}
