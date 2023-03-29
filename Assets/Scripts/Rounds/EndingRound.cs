using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingRound : Round
{
    Dictionary<GameObject, int> sortedScores = new Dictionary<GameObject, int>();
    List<GameObject> players;
    List<int> playerScores;
    int index = 0;
    int secondsElapsed = 0;
    public EndingRound()
    {
        hasMap = false;
        roundTime = 20;
        mapPrefabName = "Spinner";
        tutorialText = "THE END";

        type = roundType.ENDING;
    }

    public override void onTenSecondsBefore(object sender, EventArgs e)
    {
        //do nothing
        //we dont need this
    }

    public override void unload()
    {
        SceneManager.LoadScene("newMainMenu");
    }

    protected override void Load()
    {
        GameObject level = RoundManager.instance.levelLocation.Find("64").gameObject;
        EventManager.onRoundSecondTickEvent += tick;
        players = RoundManager.instance.currentPlayers;
        playerScores = new List<int>();
        for(int i = 0;i < players.Count; i++)
        {
            playerScores.Add(RoundManager.instance.getScore(i));
        }
        sortedScores = RoundManager.instance.getScoreTable();
        for(int i = 0; i < RoundManager.instance.currentPlayers.Count-1; i++)
        {
            for (int u= 0; u < RoundManager.instance.currentPlayers.Count-1; u++)
            {
                if(RoundManager.instance.getScore(u+1) > RoundManager.instance.getScore(u))
                {
                    GameObject tempGO = RoundManager.instance.currentPlayers[u];
                    int tempScore = RoundManager.instance.getScore(u);

                    players[u] = players[u+1];
                    playerScores[u] = playerScores[u+1];

                    players[u+1] = tempGO;
                    playerScores[u + 1] = tempScore;
                }
            }
        }
        
        for(int i = 0; i < players.Count; i++)
        {
            sortedScores.Add(players[i], playerScores[i]);
        }

        Transform levelManager = RoundManager.instance.levelLocation;
        Transform chipParent = levelManager.Find("64").Find("Chips");
        for(int i = 0; i < players.Count; i++)
        {
            chipParent.GetChild(i).GetComponent<FallingChip>().enabled = false;
            players[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ |
                RigidbodyConstraints.FreezeRotationY;
        }
        //get player platforms (get the levelmanager's child of "Chips" and set by index from there)
        //enable the falling platform behaviour
        //freeze position on the rigidbody X and Z
    }

    public void tick(object sender, RoundTickArgs e)
    {
        //after 2 seconds, get the platform players.indexOf(sortedScores.elementAt(index).key) and enable its falling
        secondsElapsed++;
        if(secondsElapsed%2 == 0)
        {
            
            makeAnotherPlatformFall();
            index++;
        }
    }

    void makeAnotherPlatformFall()
    {
        if (index == 3)
        {
            //pan camera
            return;
        }
        Transform levelManager = RoundManager.instance.levelLocation;
        Transform chipParent = levelManager.Find("64").Find("Chips");
        
        chipParent.GetChild(players.IndexOf(sortedScores.ElementAt(index).Key)).GetComponent<FallingChip>().enabled = true;
    }

  
}
