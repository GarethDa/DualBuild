using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public GameObject playerManager;
    public GameObject levelManager;
    public GameObject deathZone;
    public List<PowerUpScript> powerupManager = new List<PowerUpScript>();
    public GameObject clientPlayer;

    public int playersConnected = 2;

    public bool isHost = false;
    public bool isNetworked = false;
    void Awake()
    {
        //if(instance == null)
        {
            instance = this;

        }
    }
    
    public string getButtonString(string buttonName, GameObject player)
    {
        
            InputAction inputA = player.GetComponent<PlayerInput>().actions.FindAction(buttonName);//"Player/Fire"
            if(player.GetComponent<PlayerInput>() == null)
        {
            Debug.Log("NO PLAYERINPUT IS ATTACHED");
        }
            if(inputA == null)
        {
            Debug.Log("IPUT A IS NUL");
        }
        int bindingIndex = inputA.GetBindingIndexForControl(inputA.controls[0]);
        Debug.Log("BINDING INDEX " + bindingIndex);
        Debug.Log("INPUTA BINDINGS LENGTH " + inputA.bindings.Count);
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

    public string getActualButtonName(string buttonName, bool uppercaseFirst = false, bool standAloneButton = false)
    {
        if (standAloneButton)
        {
            if (buttonName.ToLower().Contains("north"))
            {
                buttonName = "Y";
            }
            if (buttonName.ToLower().Contains("south"))
            {
                buttonName = "A";
            }
            if (buttonName.ToLower().Contains("east"))
            {
                buttonName = "B";
            }
            if (buttonName.ToLower().Contains("west"))
            {
                buttonName = "X";
            }
            Debug.Log(buttonName);
            return buttonName;
        }
        if (buttonName.ToLower().Contains("north"))
        {
            buttonName =  "the Y button";
        }
        if (buttonName.ToLower().Contains("south"))
        {
            buttonName = "the A button";
        }
        if (buttonName.ToLower().Contains("east"))
        {
            buttonName = "the B button";
        }
        if (buttonName.ToLower().Contains("west"))
        {
            buttonName = "the X button";
            Debug.Log("X " + buttonName);
        }
        if (uppercaseFirst)
        {
            buttonName = buttonName[0].ToString().ToUpper() + buttonName.Substring(1, buttonName.Length - 1);
        }
        Debug.Log(buttonName);
        return buttonName;
    }


}
