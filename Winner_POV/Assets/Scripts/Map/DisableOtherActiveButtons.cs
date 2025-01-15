using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DisableOtherActiveButtons : MonoBehaviour
{
    List<Button> buttons = new();
    public void SetButtonsToDisable(Button[] buttonsToSet)
    {
        buttons = buttonsToSet.ToList();   
    }
    public void SetButtonsToDisable(List<Button> buttonsToSet)
    {
        buttons = buttonsToSet;
    }
    public void SetButtonsToDisable(Button[] buttonsToSet, bool removeOtherButtons)
    {
        if (removeOtherButtons)
        {
            buttons = buttonsToSet.ToList();
        }
        else
        {
            foreach (Button b in buttonsToSet)
            {
                buttons.Add(b);
            }
        }
    }
    public void SetButtonsToDisable(List<Button> buttonsToSet, bool removeOtherButtons)
    {
        if (removeOtherButtons)
        {
            buttons = buttonsToSet;
        }
        else
        {
            foreach (Button b in buttonsToSet)
            {
                buttons.Add(b);
            }
        }
    }

    public void DisableButtons()
    {
        foreach(Button b in buttons)
        {
            b.interactable = false;
        }
    }
}
