using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum currentTutorialType
{
    LOOK,
    MOVE,
    JUMP
}
public class onScreenTutorialText : MonoBehaviour
{
    public bool[] hasShownTutorialType = { false, false, false };
    private void Start()
    {
        show("Use " +  getAnalogueName(getButtonString("Player/Look")) + " to look around.");
        //hasShownTutorialType[0] = true;
    }
    public void hideTutorial(currentTutorialType toCancel)
    {
        hasShownTutorialType[(int)toCancel] = true;
        for (int i = 0; i < hasShownTutorialType.Length; i++)
        {
            if (!hasShownTutorialType[i])
            {
                return;
            }
        }
        show("");
        
    }

    public void show(string text)
    {
        GetComponent<TMP_Text>().text = text;
    }

    public void showButton(string before, string buttonName, string after)
    {
        InputAction inputA = transform.parent.transform.parent.GetComponent<PlayerInput>().actions.FindAction(buttonName);//"Player/Fire"
        int bindingIndex = inputA.GetBindingIndexForControl(inputA.controls[0]);

        string button = InputControlPath.ToHumanReadableString(inputA.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);

        show(before + button + after);
    }

    public string getButtonString(string buttonName)
    {
        InputAction inputA = transform.parent.transform.parent.GetComponent<PlayerInput>().actions.FindAction(buttonName);//"Player/Fire"
        int bindingIndex = inputA.GetBindingIndexForControl(inputA.controls[0]);

        string button = InputControlPath.ToHumanReadableString(inputA.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        return button;
    }
    public string getAnalogueName(string buttonName)
    {
        //Debug.Log(buttonName);
        buttonName = buttonName.ToLower();
        if (buttonName.Equals("delta"))
        {
            return "left mouse";
        }
        if (buttonName.Contains("left stick"))
        {
            //Debug.Log("left stick");
            return "left stick";
        }
        if (buttonName.Contains("right stick"))
        {
            return "right stick";
        }
        return buttonName;
    }

    public string getActualButtonName(string buttonName)
    {
        
        if (buttonName.ToLower().Contains("north"))
        {
            return "the Y button";
        }
        if (buttonName.ToLower().Contains("south"))
        {
            return "the A button";
        }
        if (buttonName.ToLower().Contains("east"))
        {
            return "the B button";
        }
        if (buttonName.ToLower().Contains("west"))
        {
            return "the X button";
        }
        
        return buttonName;
    }
}
