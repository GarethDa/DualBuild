using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardShower : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.onLeaderboardScore += showLeaderboard;
    }


    public void showLeaderboard(object sender, StringArgs e)
    {
        GetComponent<TMP_Text>().text = e.str;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
