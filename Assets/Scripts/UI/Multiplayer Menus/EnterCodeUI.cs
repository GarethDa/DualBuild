using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnterCodeUI : SwappableUI
{
    public TMP_InputField field;
    public TMP_Text error;
   public void buttonJoinGame()
    {
        string acceptableCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string roomKey = "";
        bool isOK = true;
        foreach(char c in field.text.ToUpper())
        {
            if(c.Equals(' '))
            {
                continue;
            }
            if (!acceptableCharacters.Contains(c.ToString()))
            {
                isOK = false;
                break;
            }
            roomKey += c.ToString();
        }
        if (!isOK)
        {
            error.text = "Please enter a valid room key!";
            return;
        }
        NetworkManager.instance.roomKey = roomKey;
        NetworkManager.instance.joinRoom();
        SwappableUIManager.instance.showUI(UIMenuType.JOIN_GAME);
    }
}
