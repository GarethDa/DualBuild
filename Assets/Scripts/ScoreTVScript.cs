using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreTVScript : MonoBehaviour
{
    string text = "";
    TextMeshPro textMesh;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int i = 1;
        foreach (GameObject obj in RoundManager.instance.currentPlayers)
        {
            if (GameManager.instance.isNetworked)
            {
                text += RoundManager.instance.playerNames[RoundManager.instance.getPlayerIndex(obj)] + " Score: " + RoundManager.instance.getScore(obj);
            }
            else
            {
                text += "Player " + i + " Score: " + RoundManager.instance.getScore(obj);
                ++i;
            }
        }
        textMesh.SetText(text);

        text = "";
    }
}
