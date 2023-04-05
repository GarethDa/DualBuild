using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerReadyCount : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.onPlayerReady += onReady;

    }


    public void onReady(object sender, IntArgs e)
    {
        if(e.i == -1)
        {
            GetComponent<TMP_Text>().text = "";
        }
        else
        {
            GetComponent<TMP_Text>().text = "\n\n" + e.i.ToString() + "/" + RoundManager.instance.currentPlayers.Count.ToString() + " Players Ready";
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
