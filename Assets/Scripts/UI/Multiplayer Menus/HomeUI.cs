using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HomeUI : SwappableUI
{
    public TMP_Text error;
    public void buttonStartGame()
    {
        if (connect())
        {
            SwappableUIManager.instance.showUI(UIMenuType.HOST_GAME);
            NetworkManager.instance.createRoom();
            
        }
    }

    public void buttonJoinGame()
    {
        if (connect())
        {
            SwappableUIManager.instance.showUI(UIMenuType.ENTER_CODE);
            

        }
    }

    private bool connect()
    {

        if (!NetworkManager.instance.connectToServer())
        {
            error.text = "Error connecting to server!";
            return false;
        }
        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
