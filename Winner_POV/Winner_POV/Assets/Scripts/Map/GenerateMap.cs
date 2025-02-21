using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GenerateMap : MonoBehaviour
{
    [SerializeField] private int mapLength = 10;

    // StepLenght = avståndet mellan två vertikala punkter
    // värdet är procenten av skärmens höjd
    [SerializeField] private float stepLength = 1.5f;

    //mapWidth = hur långt åt sidan ikonerna kan generaras
    [SerializeField] private float mapWidth = 8.5f;

    public int startingPositions = 3;

    //startingHeight = höjden som första ikonerna kommer spawnas;
    [SerializeField] private float startingHeight = -3.5f;



    [SerializeField] private GameObject mapIcon;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(GenerateTheMap());
        }
    }

    private IEnumerator GenerateTheMap()
    {
        float xPosition;

        List<Button> startingButtons = new();
        for (int i = 0; i < startingPositions; i++)
        {
            xPosition = mapWidth / startingPositions * (i + 1) - (mapWidth - mapWidth / startingPositions);
            
            GameObject nextStep = Instantiate(mapIcon, new Vector3(xPosition, startingHeight, 0), Quaternion.identity, transform);
            nextStep.GetComponent<DrawPaths>().ContinuePath(i * 2 + 1, mapLength - 1, 1, stepLength, transform, mapIcon, 0);

            startingButtons.Add(nextStep.GetComponent<Button>());

            yield return new WaitForSeconds(0.1f);
        }
        foreach(Button b in startingButtons)
        {
            b.AddComponent<DisableOtherActiveButtons>();
            b.GetComponent<DisableOtherActiveButtons>().SetButtonsToDisable(startingButtons.ToArray());
            UnityAction addToOnClick;
            addToOnClick = b.GetComponent<DisableOtherActiveButtons>().DisableButtons;
            b.onClick.AddListener(addToOnClick);

            b.interactable = true;
        }
    }
}
