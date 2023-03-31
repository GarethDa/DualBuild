using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingRound : Round
{
    Dictionary<GameObject, int> sortedScores;
    List<GameObject> players;
    List<int> playerScores;
    int index = 0;
    int secondsElapsed = 0;
    bool isDone = false;
    int secondsSinceDone = 0;
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
        //SceneManager.LoadScene("newMainMenu");
        RoundManager.instance.levelEndCamera.enabled = false;
        RoundManager.instance.levelEndCamera.GetComponentInChildren<ParticleSystem>().Stop();
        playerFallScript.instance.checkCollision = false;
        SceneManager.LoadScene(RoundManager.instance.gameEndSceneName);

    }
    protected override void Load()
    {
        RoundManager.instance.levelEndCamera.enabled = true;
    }
    protected override void LoadLate()
    {
        playerFallScript.instance.checkCollision = false;
        GameObject level = RoundManager.instance.levelLocation.Find("64(Clone)").gameObject;
        EventManager.onRoundSecondTickEvent += tick;
        players = RoundManager.instance.currentPlayers;
        playerScores = new List<int>();
        for(int i = 0;i < players.Count; i++)
        {
            playerScores.Add(RoundManager.instance.getScore(i));
        }
        sortedScores = new Dictionary<GameObject, int>();
        float zCamera = 0;
        for(int i = 0; i < players.Count-1; i++)
        {
            for (int u= 0; u < players.Count-1; u++)
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
        //Debug.Log(sortedScores.Count + " " + players.Count);
        Transform levelManager = RoundManager.instance.levelLocation;
        Transform chipParent = levelManager.Find("64(Clone)").Find("Chips");
        Transform spawnPointParent = levelManager.Find("64(Clone)").Find("SpawnPoints");
        
        for(int i = 0; i < 4; i++)
        {
            zCamera += spawnPointParent.GetChild(i).transform.position.z;
            if (i >= players.Count)
            {
                chipParent.GetChild(i).gameObject.SetActive(false);
                Debug.Log("deleted platform");
                continue;
            }
            Debug.Log(chipParent.GetChild(i).gameObject.name);
            Debug.Log(players[i].gameObject.name);
            chipParent.GetChild(i).GetComponent<FallingChip>().enabled = false;
            players[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ |
                RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            Debug.Log("PHYSICS STOPPED");
        }
        zCamera /= 4;
        Vector3 camPos = RoundManager.instance.levelEndCamera.transform.position;
        RoundManager.instance.levelEndCamera.transform.position = new Vector3(camPos.x,camPos.y,zCamera);
        
       // Debug.Break();
        //get player platforms (get the levelmanager's child of "Chips" and set by index from there)
        //enable the falling platform behaviour
        //freeze position on the rigidbody X and Z
    }

    public void tick(object sender, RoundTickArgs e)
    {
        //after 2 seconds, get the platform players.indexOf(sortedScores.elementAt(index).key) and enable its falling
        secondsElapsed++;
        if (isDone)
        {
            secondsSinceDone++;
            if(secondsSinceDone == 3)
            {
                int index = 0;
                foreach(GameObject g in players)
                {
                    int score = playerScores[index];

                    index++;
                }
            }
            if (secondsSinceDone >= 15)
            {
                unload();
            }
            return;
        }
        if (secondsElapsed%2 == 0)
        {
            
            makeAnotherPlatformFall();
            index++;
        }
    }

    void makeAnotherPlatformFall()
    {

        Transform cameraPointParent = RoundManager.instance.levelLocation.Find("64(Clone)").Find("CameraPoints");
        PreviewCameraScript prev = RoundManager.instance.levelEndCamera.GetComponent<PreviewCameraScript>();
       // prev.canMove = true;
       
        if (index >= players.Count)
        {
            RoundManager.instance.levelEndCamera.GetComponentInChildren<ParticleSystem>().Play();
            isDone = true;
            return;
        }
        int sortedScoresElement = index;
        int playerIndex = players.IndexOf(sortedScores.ElementAt(index).Key);

        prev.edges.Clear();
        prev.edges.Add(new movementLine(RoundManager.instance.levelEndCamera.transform, 0.3f, 30f));
        prev.edges.Add(new movementLine(cameraPointParent.GetChild(playerIndex), 0.3f, 0f));
        //prev.edges.Add(new movementLine(cameraPointParent.GetChild(playerIndex), 0.3f, 0f));
        prev.startMove();
        if (index >= players.Count-1)
        {
            return;
        }
            Transform levelManager = RoundManager.instance.levelLocation;
        Transform chipParent = levelManager.Find("64(Clone)").Find("Chips");
       
        Debug.Log(sortedScoresElement + " " + playerIndex);
        chipParent.GetChild(players.IndexOf(sortedScores.ElementAt(index).Key)).GetComponent<FallingChip>().enabled = true;
        chipParent.GetChild(players.IndexOf(sortedScores.ElementAt(index).Key)).GetComponent<FallingChip>().startWobbleAnimation();
        //Debug.Break();
    }

  
}
