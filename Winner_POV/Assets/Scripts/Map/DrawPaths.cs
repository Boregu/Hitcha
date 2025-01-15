using System.Collections;

using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DrawPaths : MonoBehaviour
{
    Button button;
    [SerializeField] GameObject lineDrawerObject;
    [SerializeField] private float branchChance = 0.65f;
    [SerializeField] private float mergeChance = 0.4f;
    [SerializeField] private int maxBranchLength = 3;
    [SerializeField] private float branchOffset = 1.25f;

    [Header("Branch Frequency = Antal Steg Innan en till Gren kan Skapas")]
    [SerializeField] private int branchFrequency = 2;
    private void Start()
    {
        button = GetComponent<Button>();
    }

    //Metoden för oförgrenade stigar
    public void ContinuePath(int file, int remainingSteps, int depth, float stepLength, Transform parent, GameObject mapIcon, int branchCooldown)
    {

        parent.GetComponent<MapIconManager>().AddButtonToManager(new Vector2(file, depth), GetComponent<Button>());
        GetComponent<DefinePoint>().Define(depth, remainingSteps);


        if (remainingSteps > 0)
        {
            //Genererar nästa steg i huvudgrenen

            Vector3 pos = new(transform.position.x, transform.position.y + stepLength, 0);
            DrawPaths nextStep = Instantiate(mapIcon, pos, Quaternion.identity, parent).GetComponent<DrawPaths>();

            //ritar en linje till nästa steg

            LineDrawer lineDrawer = Instantiate(lineDrawerObject, transform.position, Quaternion.identity).GetComponent<LineDrawer>();
            lineDrawer.DrawLine(transform.position, nextStep.transform.position);

            //rullar en tärning för att se om stigen kommer att förgrenas
            if (Random.Range(0f, 1f) < branchChance && branchCooldown < 1)
            {
                //ifall tärningen lyckas

                nextStep.ContinuePath(file, remainingSteps - 1, depth + 1, stepLength, parent, mapIcon, branchFrequency);

                //Slumpar vilket håll grenen kommer gå
                int branchDirection;
                if (Random.Range(1, 3) == 1)
                {
                    branchDirection = 1;
                }
                else
                {
                    branchDirection = -1;
                }

                DrawPaths branchNextStep;

                //Kollar ifall det redan finns en punkt där
                if (parent.GetComponent<MapIconManager>().GetButtonFromIndex(new Vector2(file + branchDirection, depth + 1)) == null)
                {
                    //ifall det inte finns en punkt

                    //skapar punkten
                    Vector3 branchPos = new(transform.position.x + (branchDirection * branchOffset), transform.position.y + stepLength, 0);
                    branchNextStep = Instantiate(mapIcon, branchPos, Quaternion.identity, parent).GetComponent<DrawPaths>();

                    //ritar en linje till nästa punkt

                    LineDrawer branchLineDrawer = Instantiate(lineDrawerObject, transform.position, Quaternion.identity).GetComponent<LineDrawer>();
                    branchLineDrawer.DrawLine(transform.position, branchNextStep.transform.position);

                    branchNextStep.ContinuePath(file + branchDirection, remainingSteps - 1, depth + 1, stepLength, parent, mapIcon, true, 1);
                }
                else
                {
                    //ifall det redan finns en punkt

                    //skapar en referens till punkten
                    branchNextStep = parent.GetComponent<MapIconManager>().GetButtonFromIndex(new Vector2(file + branchDirection, depth + 1)).GetComponent<DrawPaths>();

                    //ritar en linje till punkten
                    LineDrawer branchLineDrawer = Instantiate(lineDrawerObject, transform.position, Quaternion.identity).GetComponent<LineDrawer>();
                    branchLineDrawer.DrawLine(transform.position, branchNextStep.transform.position);
                }

                Button[] nextSteps = { nextStep.GetComponent<Button>(), branchNextStep.GetComponent<Button>() };

                foreach (Button b in nextSteps)
                {
                    if (b.GetComponent<DisableOtherActiveButtons>() == null)
                    {
                        b.AddComponent<DisableOtherActiveButtons>();
                        UnityAction addToOnClick;
                        addToOnClick = b.GetComponent<DisableOtherActiveButtons>().DisableButtons;
                        b.onClick.AddListener(addToOnClick);
                    }

                    b.GetComponent<DisableOtherActiveButtons>().SetButtonsToDisable(nextSteps, false);
                }

                GetComponent<EnableNextPoints>().SetNextButtons(nextSteps);
            }
            else
            {
                //ifall den inte lyckas

                nextStep.ContinuePath(file, remainingSteps - 1, depth + 1, stepLength, parent, mapIcon, branchCooldown - 1);

                Button[] nextSteps = { nextStep.GetComponent<Button>() };
                GetComponent<EnableNextPoints>().SetNextButtons(nextSteps);
            }

        }
        else
        {
            EndPath(stepLength);
        }
    }

    // Overload metod för förgrenade stigar
    public void ContinuePath(int file, int remainingSteps, int depth, float stepLength, Transform parent, GameObject mapIcon, bool isBranch, int branchLength)
    {

        parent.GetComponent<MapIconManager>().AddButtonToManager(new Vector2(file, depth), GetComponent<Button>());
        GetComponent<DefinePoint>().Define(depth, remainingSteps);

        if (remainingSteps > 0)
        {
            if (Random.Range(0f, 1f) < mergeChance || maxBranchLength <= branchLength)
            {
                StartCoroutine(MergePath(parent.gameObject, file, depth));
            }
            else
            {
                if (parent.GetComponent<MapIconManager>().GetButtonFromIndex(new Vector2(file, depth + 1)) == null)
                {
                    Vector3 pos = new(transform.position.x, transform.position.y + stepLength, 0);

                    DrawPaths nextStep = Instantiate(mapIcon, pos, Quaternion.identity, parent).GetComponent<DrawPaths>();

                    LineDrawer lineDrawer = Instantiate(lineDrawerObject, transform.position, Quaternion.identity).GetComponent<LineDrawer>();
                    lineDrawer.DrawLine(transform.position, nextStep.transform.position);

                    nextStep.ContinuePath(file, remainingSteps - 1, depth + 1, stepLength, parent, mapIcon, true, branchLength + 1);

                    Button[] nextSteps = { nextStep.GetComponent<Button>() };
                    GetComponent<EnableNextPoints>().SetNextButtons(nextSteps);
                }
                else
                {
                    DrawPaths nextStep = parent.GetComponent<MapIconManager>().GetButtonFromIndex(new Vector2(file, depth + 1)).GetComponent<DrawPaths>();

                    LineDrawer lineDrawer = Instantiate(lineDrawerObject, transform.position, Quaternion.identity).GetComponent<LineDrawer>();
                    lineDrawer.DrawLine(transform.position, nextStep.transform.position);

                    Button[] nextSteps = { nextStep.GetComponent<Button>() };
                    GetComponent<EnableNextPoints>().SetNextButtons(nextSteps);
                }
            }
        }
        else
        {
            EndPath(stepLength);
        }
    }

    private void EndPath(float stepLength)
    {
        LineDrawer lineDrawer = Instantiate(lineDrawerObject, transform.position, Quaternion.identity).GetComponent<LineDrawer>();
        lineDrawer.DrawLine(transform.position, new Vector2(0, transform.position.y + stepLength));
    }

    private IEnumerator MergePath(GameObject parent, int file, int depth)
    {
        yield return new WaitForSeconds(0.3f);
        Button[] nextSteps;
        if (Random.Range(1, 3) == 1 && file > 0 && file < (parent.GetComponent<GenerateMap>().startingPositions - 1) * 2 + 2)
        {
            Button leftMerge;
            Button rightMerge;

            Vector2 mergePointCoordinates = new(Mathf.Clamp(file - 1, 1, (parent.GetComponent<GenerateMap>().startingPositions - 1) * 2 + 1), depth);

            if (parent.GetComponent<MapIconManager>().GetButtonFromIndex(mergePointCoordinates).GetComponent<EnableNextPoints>().CheckIfPointLeadsTo(new Vector2(file, depth + 1), parent) == true)
            {
                leftMerge = parent.GetComponent<MapIconManager>().GetButtonFromIndex(new Vector2(file, depth + 1));
            }
            else
            {
                leftMerge = parent.GetComponent<MapIconManager>().GetButtonFromIndex(new Vector2(file - 1, depth + 1));
            }

            mergePointCoordinates.x = Mathf.Clamp(file + 1, 1, (parent.GetComponent<GenerateMap>().startingPositions - 1) * 2 + 1);

            if (parent.GetComponent<MapIconManager>().GetButtonFromIndex(mergePointCoordinates).GetComponent<EnableNextPoints>().CheckIfPointLeadsTo(new Vector2(file, depth + 1), parent) == true)
            {
                rightMerge = parent.GetComponent<MapIconManager>().GetButtonFromIndex(new Vector2(file, depth + 1));
            }
            else
            {
                rightMerge = parent.GetComponent<MapIconManager>().GetButtonFromIndex(new Vector2(file + 1, depth + 1));
            }

            nextSteps = new Button[] { leftMerge, rightMerge };

            foreach (Button b in nextSteps)
            {
                if (b.GetComponent<DisableOtherActiveButtons>() == null)
                {
                    b.AddComponent<DisableOtherActiveButtons>();
                    UnityAction addToOnClick;
                    addToOnClick = b.GetComponent<DisableOtherActiveButtons>().DisableButtons;
                    b.onClick.AddListener(addToOnClick);
                }

                b.GetComponent<DisableOtherActiveButtons>().SetButtonsToDisable(nextSteps, false);
            }

            GetComponent<EnableNextPoints>().SetNextButtons(nextSteps);
        }
        else
        {
            int mergeDirection;
            if (Random.Range(1, 3) == 1)
            {
                mergeDirection = 1;
            }
            else
            {
                mergeDirection = -1;
            }

            Vector2 mergePointCoordinates = new(Mathf.Clamp(file + mergeDirection, 1, (parent.GetComponent<GenerateMap>().startingPositions - 1) * 2 + 1), depth + 1);

            //kollar ifall stigen kommer korsa med en annan
            if (parent.GetComponent<MapIconManager>().GetButtonFromIndex(new Vector2(mergePointCoordinates.x, mergePointCoordinates.y - 1)).GetComponent<EnableNextPoints>().CheckIfPointLeadsTo(new Vector2(file, depth + 1), parent) == true)
            {
                nextSteps = new Button[] { parent.GetComponent<MapIconManager>().GetButtonFromIndex(new Vector2(file, depth + 1)) };
                GetComponent<EnableNextPoints>().SetNextButtons(nextSteps);
            }
            else
            {
                nextSteps = new Button[] { parent.GetComponent<MapIconManager>().GetButtonFromIndex(mergePointCoordinates) };
                GetComponent<EnableNextPoints>().SetNextButtons(nextSteps);
            }
        }

        //ritar linjer till punkterna
        foreach(Button b in nextSteps)
        {
            LineDrawer lineDrawer = Instantiate(lineDrawerObject, transform.position, Quaternion.identity).GetComponent<LineDrawer>();
            lineDrawer.DrawLine(transform.position, b.transform.position);
        }
    }
}
