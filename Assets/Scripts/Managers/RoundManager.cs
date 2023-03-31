using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance = null;//singleton
    List<Round> nextRounds = new List<Round>();//rounds that are being played
    List<Round> currentRounds = new List<Round>();//rounds that get queued for next time
    int currentRoundSeconds = 0;//seconds the current round should last
    public int currentRoundSecondsElapsed = 0;//current elapsed time

    //public TextMeshProUGUI clockObject; //clock text on canvas
    public Transform intermissionLocation; //where to spawn players for intermission
    public Transform levelLocation; //where to spawn players when a round starts
    public GameObject deathLocation; //where players go when they die (AKA purgatory)
    public GameObject tempLocation; //where players go when they die (AKA purgatory)

    public List<GameObject> currentPlayers = new List<GameObject>();
    public List<string> playerNames = new List<string>();

    public int deadPlayers = 0;
    public int totalPlayers = 1;
    int playersReady = 0;
    int gameRoundsCompleted = 0;
    public string gameEndSceneName;
    public int roundsToPlay = 6;
    public Camera levelCam;
    public Camera camToDisable;

    List<roundPair> levelCombinations = new List<roundPair>();

    public List<DynamicUIComponent> tutorials;
    public List<TextMeshProUGUI> tutorialText;
    public List<Transform> tutorialOffScreen;
    public List<Transform> tutorialOnScreen;

    public int roundsBetweenPowerups = 3;
    int roundsSinceLastPowerup = 0;
    bool inPreview = false;
    int secondsToAddBack = 0;
    public PowerupGiver lastPlacePowerupGiver;
    public PowerupGiver mainPowerupGiver;
    public powerUpList givingPowerup;
    bool skipPreview = false;
    bool hasModifiedLevels = false;
    public int playerIndexOffset = 0;
    bool gameHasStarted = false;
    Dictionary<GameObject, int> scoreTable = new Dictionary<GameObject, int>();
    List<GameObject> deadPlayerList = new List<GameObject>();
    public Camera levelEndCamera;
    public List<DynamicUIComponent> UIScores;

    public bool hasPlayerDied(GameObject game)
    {
        return deadPlayerList.Contains(game);
    }


    //public roundType roundOne = roundType.NONE;
    //public roundType roundTwo = roundType.NONE;

    public void addPlayer(GameObject obj, string name)
    {
        currentPlayers.Add(obj);
        playerNames.Add(name);
        addScore(obj, 0);
    }

    

    public Dictionary<GameObject, int> getScoreTable()
    {
        return scoreTable;
    }

    public void addScore(GameObject g, int i)
    {
        if (!scoreTable.ContainsKey(g))
        {
            scoreTable.Add(g, 0);
        }
        scoreTable[g] += i;
    }

    public int getScore(GameObject g)
    {
        return scoreTable[g];
    }

    public GameObject getPlayer(int index)
    {
        int i = 0;
        foreach (GameObject g in scoreTable.Keys)
        {
            if (i == index)
            {
                return g;
            }
            i++;
        }
        return null;
    }

    public int getScore(int index)
    {
        int i = 0;
        foreach(GameObject g in scoreTable.Keys)
        {
            if(index == i)
            {
                return scoreTable[g];
            }
            i++;
        }
        return -1;
    }

    public void setGameStarted(bool start)
    {
       
        gameHasStarted = start;
    }

    private void Awake() //singleton
    {
        if (instance != null)
        {
            return;
        }
        totalPlayers = 0;
        instance = this;
        gameRoundsCompleted = 0;
        
    }

    private void Start()
    {
        //temporarily starting with an intermission for testing purposes
        playerFallScript.instance.checkCollision = true;
        levelCam.enabled = false;
        camToDisable.enabled = true;
        levelEndCamera.enabled = false;
        levelCombinations = new List<roundPair>();
       // mainPowerupGiver.setPowerup(powerUpList.None);
        //lastPlacePowerupGiver.setPowerup(powerUpList.None);
        addRound(new Intermission());
        startRound("Game starting");
        
        
    }

    public bool loadLevelExpress(int levelNumber)
    {
        
        //if (levelNumber != 2)
        //{
            
        //}
        
        nextRounds.Clear();
        if (levelNumber==2)
        {
            if (shouldEndTheGame())
            {
                NetworkManager.instance.ignoreAndSendTCPMessage(NetworkManager.instance.getInstructionCode(InstructionType.ADD_SCORE) + getScore(0).ToString());
                Debug.Log("SENT SCORE TO END GAME");
                //pu tthis in load level express so you can check there if your rounds are over cause the server cant do it and when you die, you automatically request a level from the server
                //cant continue cause we need everyones score in first
                //return false;
                return false;
            }
            
        }
        else
        {
            gameRoundsCompleted++;
            roundsSinceLastPowerup++;
        }
        
        if (levelNumber == 2 || levelNumber == 64)
        {
            addRounds(getRoundsByInt(levelNumber));//no preview
            
        }
        else
        {
            addRounds(getRoundsByInt(levelNumber));
            //addRound(new PreviewRound(getRoundsByInt(levelNumber)));
        }
        return true;


    }

    public void onPlayerEnterReadyZone()
    {
        if (GameManager.instance.isNetworked)
        {
            Debug.Log("SENT READY");
            NetworkManager.instance.ignoreAndSendTCPMessage(NetworkManager.instance.getInstructionCode(InstructionType.READY) + 1.ToString());
            return;
        }
        Debug.Log("a");
        playersReady++;
        if (currentRoundSeconds - currentRoundSecondsElapsed <= 10)
        {
            Debug.Log("b");
            return;
        }
        Debug.Log("c");
        if (playersReady == totalPlayers)
        {
            setEveryoneReady();
        }
    }

    public void setEveryoneReady()
    {
        if (currentRoundSeconds - currentRoundSecondsElapsed <= 10)
        {
            return;
        }
        secondsToAddBack = (currentRoundSeconds - currentRoundSecondsElapsed);
        setRoundTime(5);
        EventManager.onRoundSecondTickEvent?.Invoke(null, new RoundTickArgs(currentRoundSecondsElapsed, currentRoundSeconds - currentRoundSecondsElapsed, currentRoundSeconds));
        EventManager.onOnAirShowEvent?.Invoke(null, System.EventArgs.Empty);

    }

    public void setEveryoneNotReady()
    {
        if (currentRoundSeconds - currentRoundSecondsElapsed <= 10 && secondsToAddBack == 0)
        {
            return;
        }
        if (!currentRoundsHaveIntermission())
        {
            return;
        }
        if (playersReady != totalPlayers && secondsToAddBack != 0)
        {

            setRoundTime(secondsToAddBack);
            EventManager.onRoundSecondTickEvent?.Invoke(null, new RoundTickArgs(currentRoundSecondsElapsed, currentRoundSeconds - currentRoundSecondsElapsed, currentRoundSeconds));
            EventManager.onOnAirHideEvent?.Invoke(null, System.EventArgs.Empty);
            secondsToAddBack = 0;
        }
    }
    public void onPlayerExitReadyZone()
    {
        Debug.Log("SENT NOT READY");
        if (GameManager.instance.isNetworked)
        {
            NetworkManager.instance.ignoreAndSendTCPMessage(NetworkManager.instance.getInstructionCode(InstructionType.READY) + 0.ToString());
            return;
        }
        playersReady--;
        setEveryoneNotReady();
        
    }

    public bool isInPreview()
    {
        return inPreview;
    }

    public void addRounds(List<Round> r)
    {
        foreach(Round round in r)
        {
            addRound(round);
        }
    }

    public void addRound(Round r)
    {
        nextRounds.Add(r);
        Debug.Log("ROUND ADDED " + r.getType().ToString());
    }

    public void addToDeath(GameObject g)
    {
        deadPlayerList.Add(g);
        int scoreToAdd = 5 - (5 - deadPlayerList.Count);
        addScore(g, scoreToAdd);
    }

    public void onDeath(object sender, PlayerArgs e)
    {
        addToDeath(e.player);
        if (GameManager.instance.isNetworked)
        {
            
            NetworkManager.instance.ignoreAndSendTCPMessage(NetworkManager.instance.getInstructionCode(InstructionType.PLAYER_DIED) + e.player.GetInstanceID().ToString());
            return;
        }
        deadPlayers++;
        //Debug.log("$ONDEWATH" + deadPlayers.ToString() + " " + (totalPlayers - 1).ToString());
        if (deadPlayers >= totalPlayers - 1)
        {
            endRound("All players died");
            deadPlayers = 0;
        }
    }

    public int getNextRoundNumber()
    {
        int returner = 0;
        foreach (Round r in nextRounds)
        {
            returner += (int)r.getType();
        }
        return returner;
    }

    public int getCurrentRoundNumber()
    {
        if (currentRoundsHaveIntermission())
        {
            return 2;
        }
        int returner = 0;
        foreach (Round r in currentRounds)
        {
            returner += (int)r.getType();
        }
        if(returner == 0)
        {
            foreach (Round r in nextRounds)
            {
                returner += (int)r.getType();
            }
        }
        Debug.Log("RETURNER " + returner);
        return returner;
    }
    public void startRound(string why)
    {
        Debug.Log(why);
        deadPlayerList.Clear();
        playerFallScript.instance.resetFallenPlayers();
        
        /*
        if(roundOne != roundType.NONE)
        {
            nextRounds.Clear();
            nextRounds.Add(getRoundByRoundType(roundOne));
            nextRounds.Add(getRoundByRoundType(roundTwo));
            nextRounds.Add(new PreviewRound(nextRounds));
        }
        */
        //Debug.log("$-------------");
        currentRoundSeconds = 0;//reset time of rounds
        currentRoundSecondsElapsed = 0;
        secondsToAddBack = 0;
        deadPlayers = 0;
        int toLoad = 0;
        int roundSeconds = 0;
        bool sendToLevel = true;
        List<roundType> roundTypes = new List<roundType>();
        foreach (Round r in nextRounds)
        {

            roundTypes.Add(r.getType());
            if (r is PreviewRound)
            {
                PreviewRound preview = (PreviewRound)r;

                toLoad += (int)preview.nextRounds[0].getType();
                toLoad += (int)preview.nextRounds[1].getType();
                roundSeconds += preview.getRoundTime();
                Debug.Log(preview.nextRounds[0].getType().ToString());
                Debug.Log(preview.nextRounds[1].getType().ToString());
                Debug.Log("adding here");
                sendToLevel = false;


            }
            else
            {

                toLoad += (int)r.getType();
                roundSeconds += r.getRoundTime();


            }
            r.load();

            currentRounds.Add(r);
        }

        //Camera.main.enabled = false;
        //toLoad = 24;
        if (nextRoundsHaveIntermission())//if next round is intermission, go to intermission
        {
            //Debug.log("$START HAS INTERMISSION");
            sendPlayersToIntermission();

        }
        else
        {
            //Debug.log("$START HAS NO INTERMISSION");
            Debug.Log(toLoad);
            List<Transform> placeToSend = loadLevel(toLoad);
            foreach(Round r in currentRounds)
            {
                r.loadLate();
            }
            if (sendToLevel)
            {
                sendPlayersToLevel(placeToSend);
                Debug.Log("SENT TO LEVEL");
            }
            else
            {
                //send players to purgatory to wait
                sendPlayersToLocation(new List<Transform> { tempLocation.transform });
            }

        }

        //reset lists for next round
       // Debug.Log(roundSeconds);
        currentRoundSeconds = roundSeconds;
        updateScreenClock();
        roundSeconds = 0;

        nextRounds.Clear();
        //setPowerUps();
        //subscribe to events
        EventManager.onPlayerFell += onDeath;
        EventManager.onSecondTickEvent += secondTick;//subscribe to the second ticking event
        EventManager.onRoundStart?.Invoke(null, new RoundArgs(roundTypes.ToArray())); ;//invoke the round start event for other scripts

    }

    public bool shouldEndTheGame()
    {
        int networkBias = 0;
        if (GameManager.instance.isNetworked)
        {
            networkBias = roundsToPlay-1;
        }
        Debug.Log(gameRoundsCompleted + " " + (roundsToPlay + networkBias));
        if (gameRoundsCompleted >= roundsToPlay + networkBias)
        {
            return true;
        }
        return false;   
     }

    public bool endRoundCleanup()
    {
        EventManager.onSecondTickEvent -= secondTick;//unsubscribe from the second tick event (so the clock stops)
        EventManager.onPlayerFell -= onDeath;
        EventManager.onRoundEnd?.Invoke(null, new RoundArgs(new roundType[] { roundType.NONE, roundType.NONE }));
        for (int i = 0; i < GameManager.instance.levelManager.transform.childCount; i++)
        {
            if (GameManager.instance.levelManager.transform.GetChild(i).gameObject.tag.Equals("LevelPersistent"))
            {
                continue;
            }
            Destroy(GameManager.instance.levelManager.transform.GetChild(i).gameObject);

        }
        //unload rounds
        foreach (Round r in currentRounds)
        {
            r.unload();
        }
        if (!currentRoundsHaveIntermission() && !currentRoundsHavePreview() && !GameManager.instance.isNetworked)
        {
            gameRoundsCompleted++;
            roundsSinceLastPowerup++;
            //setPowerUps();
        }
       
        if (shouldEndTheGame() && !GameManager.instance.isNetworked)
        {
            //TODO fix this for networked, just request the level 64 from the server

            

            //switch scene
            gameRoundsCompleted = 0;
            nextRounds.Clear();
            nextRounds.Add(new EndingRound());
            startRound("ending");
            return false;
            //Debug.Log("ENDING");
            //SceneManager.LoadScene(gameEndSceneName);
            //return;
        }
        return true;
    }

    public void endRound(string why)
    {

        Debug.Log("$WHY: " + why);
        if (!endRoundCleanup())
        {
            Debug.Log("no continuing end round");
            return;
        }
        //deadPlayers = 0;
        //remove children from the levelManager (destroys the level that was spawned in)

        hasModifiedLevels = false;
        if (GameManager.instance.isNetworked)
        {
            requestNewLevelFromServer();
            return;
        }

        //check if its intermission

        if (currentRoundsHavePreview() && !skipPreview && !hasModifiedLevels)
        {
            
            
            
                PreviewRound preview = (PreviewRound)currentRounds[0];
                currentRounds.Clear();
                addRound(preview.nextRounds[0]);
                addRound(preview.nextRounds[1]);
                startRound("create preview");
            
            
            inPreview = false;
        }
        else if (!currentRoundsHaveIntermission())
        {
           
            
            
                currentRounds.Clear();//to clear it before next rounds get loaded (but must be available to check for intermission above)
                Debug.Log("$END ROUND HAS NO INTERMISSION");
                
                addRound(new Intermission());
                startRound("create intermission");
            
            
            inPreview = false;
        }
        else
        {//start rounds
            
            
                currentRounds.Clear();
                Debug.Log("$END ROUND INTERMISSION");
                generateNextRoundLevels();
                startRound("load actual level");
            
            
        }
        
    }

    private void generateNextRoundLevels(int offset = 0)
    {
        if (levelCombinations.Count == 0)
        {
            //generate level combinations again
            List<roundType> possibleRounds = new List<roundType>();

            
            foreach (roundType r in System.Enum.GetValues(typeof(roundType)))//make a list of all roundTypes
            {
                if (r == roundType.NONE || r == roundType.INTERMISSION || r == roundType.ENDING)
                {
                    continue;
                }
                possibleRounds.Add(r);
            }

            foreach (roundType outerType in possibleRounds)
            {
                foreach (roundType innerType in possibleRounds)
                {
                    if (outerType != innerType)
                    {
                        //add to list if list doesnt contain its code
                        int code = (int)outerType + (int)innerType;

                        bool listHasCode = false;

                        //scan list to see if there is a combination with the same code
                        foreach (roundPair r in levelCombinations)
                        {
                            if (r.roundCode == code)
                            {
                                listHasCode = true;
                                break;
                            }
                        }
                        if (listHasCode)
                        {
                            continue;
                        }
                        levelCombinations.Add(new roundPair(getRoundByRoundType(outerType), getRoundByRoundType(innerType), code));
                    }
                }
            }
            

            List<roundPair> randomizedCombinations = new List<roundPair>();
            foreach (roundPair roundPair in levelCombinations)
            {
                randomizedCombinations.Add(roundPair);
            }
            levelCombinations.Clear();
            while (randomizedCombinations.Count != 0)
            {
                int randomIndex = Random.Range(0, randomizedCombinations.Count);
                levelCombinations.Add(randomizedCombinations[randomIndex]);
                randomizedCombinations.RemoveAt(randomIndex);
            }


        }



        int index = (gameRoundsCompleted+offset) % levelCombinations.Count;

        //add preview round first
        List<Round> playingRounds = new List<Round>();
        playingRounds.Add(levelCombinations[index].getRoundOne());
        playingRounds.Add(levelCombinations[index].getRoundTwo());

        bool contained = false;
        int a = (int)levelCombinations[index].getRoundOne().getType();
        int b = (int)levelCombinations[index].getRoundTwo().getType();
        Debug.Log(a + b);
        int[] possibleLevels = { 12, 20, 36, 24, 40, 48 };
        for(int i = 0; i < 6; i++)
        {
            

            if (possibleLevels[i] == (a + b))
            {
                contained = true;
               
                break;
            }
        }
        if (!contained)
        {
            Debug.Log("NOT A VALID LEVEL");
            generateNextRoundLevels(1+offset);
            return;
        }
       
        if (!skipPreview && !hasModifiedLevels)
        {
            addRound(new PreviewRound(playingRounds));
            inPreview = true;
        }
        else
        {
            if (!hasModifiedLevels)
            {
                addRound(playingRounds[0]);
                addRound(playingRounds[1]);
            }

            //startRound("generatednew levels");
        }



    }

    private void setPowerUps()
    {
        //TODO: get from server and server sets it up the same way as the round chooser
        if (givingPowerup != powerUpList.None)
        {
            assignPowerUp(mainPowerupGiver, powerUpList.None, givingPowerup);
            assignPowerUp(lastPlacePowerupGiver, powerUpList.None, givingPowerup);
            return;
        }
        powerUpList givenPowerUp = powerUpList.None;
        if (roundsSinceLastPowerup >= roundsBetweenPowerups)
        {
            //give players the choice
            givenPowerUp = assignPowerUp(mainPowerupGiver, givenPowerUp, powerUpList.None);

        }
        assignPowerUp(lastPlacePowerupGiver, givenPowerUp, powerUpList.None);
    }
    private Round getRoundByRoundType(roundType r)
    {
        if (r == roundType.BUMPER)
        {
            return (new BumperRound());
        }
        if (r == roundType.DODGEBALL)
        {
            return (new DodgeballRound());
        }
        if (r == roundType.PACHINKO_BALL)
        {
            return (new PachinkoRound());
        }
        if (r == roundType.FALLING_PLATFORMS)
        {
            return (new FallingPlatformRound());
        }
        if (r == roundType.ENDING)
        {
            return (new EndingRound());
        }
        return new Intermission();
    }

    public List<Round> getRoundsByInt(int i)
    {
        if(i == 2)
        {
            return new List<Round> { getRoundByRoundType(roundType.INTERMISSION) };
        }
        if(i == 12)
        {
            return new List<Round> { getRoundByRoundType(roundType.FALLING_PLATFORMS),
            getRoundByRoundType(roundType.PACHINKO_BALL)};
        }
        if (i == 20)
        {
            return new List<Round> { getRoundByRoundType(roundType.FALLING_PLATFORMS),
            getRoundByRoundType(roundType.BUMPER)};
        }
        if (i == 36)
        {
            return new List<Round> { getRoundByRoundType(roundType.FALLING_PLATFORMS),
            getRoundByRoundType(roundType.DODGEBALL)};
        }
        if (i == 24)
        {
            return new List<Round> { getRoundByRoundType(roundType.PACHINKO_BALL),
            getRoundByRoundType(roundType.BUMPER)};
        }
        if (i == 40)
        {
            return new List<Round> { getRoundByRoundType(roundType.PACHINKO_BALL),
            getRoundByRoundType(roundType.DODGEBALL)};
        }
        if (i == 48)
        {
            return new List<Round> { getRoundByRoundType(roundType.BUMPER),
            getRoundByRoundType(roundType.DODGEBALL)};
        }
        if (i == 64)
        {
            return new List<Round> { getRoundByRoundType(roundType.ENDING) };
        }

        return new List<Round> {
            getRoundByRoundType(roundType.ENDING) };
            
    }

    public List<Transform> loadLevel(int number)
    {//instantiate from resources/load
        Debug.Log(number);
        GameObject level = Instantiate(Resources.Load<GameObject>("Levels/" + number.ToString()));
        GameObject deathZone = Instantiate(Resources.Load<GameObject>("Levels/DeathZone"));
        List<Transform> spawnPoints = new List<Transform>();
        Transform levelSpawnParent = level.transform.Find("SpawnPoints");
        for (int i = 0; i < 4; i++)
        {
            spawnPoints.Add(levelSpawnParent.GetChild(i));
        }
        level.transform.SetParent(GameManager.instance.levelManager.transform);
        level.transform.position = levelLocation.transform.position + level.transform.position;
        deathZone.transform.SetParent(GameManager.instance.levelManager.transform);
        deathZone.transform.position = level.transform.parent.transform.position + Vector3.up * 90;
        GameManager.instance.deathZone = deathZone;
        deathLocation = deathZone;
        return spawnPoints;
    }

    protected bool nextRoundsHaveIntermission()//check next rounds for an intermission round
    {
        bool hasIntermission = false;

        foreach (Round r in nextRounds)
        {

            if (r.getType() == roundType.INTERMISSION)
            {
                hasIntermission = true;
            }
        }

        return hasIntermission;
    }
    protected bool currentRoundsHaveIntermission()//check current rounds for an intermission round
    {
        bool hasIntermission = false;

        foreach (Round r in currentRounds)
        {

            if (r.getType() == roundType.INTERMISSION)
            {
                hasIntermission = true;
            }
        }

        return hasIntermission;
    }

    protected bool currentRoundsHavePreview()//check current rounds for an preview round
    {
        bool hasPreview = false;

        foreach (Round r in currentRounds)
        {

            if (r.getType() == roundType.NONE)
            {
                hasPreview = true;
            }
        }

        return hasPreview;
    }

    protected void sendPlayersToLevel(List<Transform> t)
    {
        //Debug.log("$SEND TO LEVEL");
        sendPlayersToLocation(t);
    }

    public void sendPlayersToIntermission()
    {
        //Debug.log("$SEND TO INTERMISSION");
        sendPlayersToLocation(new List<Transform> { intermissionLocation });
    }

    protected void sendPlayersToLocation(List<Transform> t)
    {
        
        for (int i = 0; i < currentPlayers.Count; i++)//for (int i = 0; i < GameManager.instance.playerManager.transform.childCount; i++)
        {
            /*
            if (GameManager.instance.isNetworked)
            {
                if (currentPlayers[i].gameObject.tag == "NetworkedPlayer")
                {
                    continue;
                }
            }
            */
            //Vector3 offset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            //GameManager.instance.playerManager.transform.GetChild(i).transform.position = teleportLocation + offset;
            currentPlayers[i].transform.position = t[(i+playerIndexOffset) % t.Count].position;// + offset;

            
        }
    }
    public void secondTick(object sender, System.EventArgs e)//called every second
    {
        if (!gameHasStarted && GameManager.instance.isNetworked)
        {
            return;
        }
        currentRoundSecondsElapsed++;
        if (secondsToAddBack > 0)
        {
            secondsToAddBack--;
        }
        updateScreenClock();

        ////Debug.log("SEC LEFT " +( currentRoundSeconds - currentRoundSecondsElapsed).ToString());
        if (currentRoundSeconds - currentRoundSecondsElapsed == 10)
        {
            EventManager.onTenSecondsBeforeRoundEndEvent?.Invoke(null, System.EventArgs.Empty);
        }
       
        EventManager.onRoundSecondTickEvent?.Invoke(null, new RoundTickArgs(currentRoundSecondsElapsed, currentRoundSeconds - currentRoundSecondsElapsed, currentRoundSeconds));
        if (currentRoundSecondsElapsed == currentRoundSeconds)
        {
            endRound("Time's up!");
        }

    }

    public void requestNewLevelFromServer()
    {
        NetworkManager.instance.ignoreAndSendTCPMessage(NetworkManager.instance.getInstructionCode(InstructionType.REQUEST_LEVEL) + getCurrentRoundNumber());

    }

    public void setRoundTime(int secondsUntilEnd)
    {
        currentRoundSecondsElapsed = (currentRoundSeconds - secondsUntilEnd);
        updateScreenClock();
    }


    void updateScreenClock()//function to update the clock
    {
        int counter = currentRoundSeconds - currentRoundSecondsElapsed;
        int minutes = 0;
        int seconds = 0;
        string extraSecondZero = "";
        while (counter > 59)
        {
            counter -= 60;
            minutes++;
        }
        seconds = counter;
        if (seconds < 10)
        {
            extraSecondZero = "0";
        }



        //clockObject.text = minutes.ToString() + ":" + extraSecondZero + seconds.ToString();
    }

    public powerUpList assignPowerUp(PowerupGiver g, powerUpList exclude, powerUpList give = powerUpList.None)
    {
        if (give != powerUpList.None)
        {
            g.setPowerup(give);
            return give;
        }
        //generate level combinations again
        List<powerUpList> possiblePowerUps = new List<powerUpList>();


        foreach (powerUpList r in System.Enum.GetValues(typeof(powerUpList)))//make a list of all roundTypes
        {
            if (r == powerUpList.None || r == exclude)
            {
                continue;
            }

            possiblePowerUps.Add(r);
        }
        powerUpList returner = possiblePowerUps[Random.Range(0, possiblePowerUps.Count)];
        g.setPowerup(returner);
        return returner;
    }
}

public struct roundPair
{
    Round roundOne;
    Round roundTwo;
    public int roundCode;

    public roundPair(Round one, Round two, int code)
    {
        roundOne = one;
        roundTwo = two;
        roundCode = code;
    }

    public Round getRoundOne()
    {
        return roundOne;
    }
    public Round getRoundTwo()
    {
        return roundTwo;
    }
}


