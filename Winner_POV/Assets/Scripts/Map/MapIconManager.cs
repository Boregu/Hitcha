using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapIconManager : MonoBehaviour
{
    [SerializeField] List<Vector2> mapIconIndex = new();
    [SerializeField] List<Button> mapIcon = new();

    public void AddButtonToManager(Vector2 index, Button button)
    {
        if (!mapIconIndex.Contains(index))
        {
            mapIconIndex.Add(index);
            mapIcon.Add(button);
        }
        else
        {
            print("Button at " + index + " Already Exists!");
        }
    }

    public Button GetButtonFromIndex(Vector2 index)
    {
        if (mapIconIndex.Contains(index) == true)
        {
            return mapIcon[mapIconIndex.IndexOf(index)];
        }
        else
        {
            return null;
        }
    }
}
