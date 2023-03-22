using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewPlayerShower : MonoBehaviour
{
    List<string> connectedPlayerNames = new List<string>();
    void Start()
    {
        EventManager.onNewPlayerJoined += showKey;
        GetComponent<TMP_Text>().text = "No players connected";
    }

    public void showKey(object sender, StringArgs e)
    {
        connectedPlayerNames.Add( e.str);
        changeText();
    }

    public void changeText()
    {
        string text = "Players connected: " + connectedPlayerNames.Count.ToString() + "/4\n";
        foreach(string s in connectedPlayerNames)
        {
            text += s + "\n";
        }

        GetComponent<TMP_Text>().text = text;
    }
}
