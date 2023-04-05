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
        foreach (GameObject obj in RoundManager.instance.currentPlayers)
        {
            text += RoundManager.instance.playerNames[RoundManager.instance.getPlayerIndex(obj)] + " Score: " + RoundManager.instance.getScore(obj);
        }
        textMesh.SetText(text);

        text = "";
    }
}
