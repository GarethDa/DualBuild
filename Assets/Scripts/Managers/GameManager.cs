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
        if(instance == null)
        {
            instance = this;

        }
    }
    
    public string getButtonString(string buttonName, GameObject player)
    {
        
            InputAction inputA = player.GetComponent<PlayerInput>().actions.FindAction(buttonName);//"Player/Fire"
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
