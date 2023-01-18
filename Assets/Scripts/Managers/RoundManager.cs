using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance = null;//singleton
    List<Round> nextRounds = new List<Round>();//rounds that are being played
    List<Round> currentRounds = new List<Round>();//rounds that get queued for next time
    int currentRoundSeconds = 0;//seconds the current round should last
    public int currentRoundSecondsElapsed = 0;//current elapsed time

    public TextMeshProUGUI clockObject;//clock text on canvas
    public Transform intermissionLocation;//where to spawn players for intermission
    public Transform levelLocation;//where to spawn players when a round starts
    public GameObject deathLocation;//where players go when they die (AKA purgatory)

    public List<GameObject> currentPlayers = new List<GameObject>();

    public int deadPlayers = 0;
    public int totalPlayers = 1;
    int gameRoundsCompleted = 0;
    public string gameEndSceneName;

    List<roundPair> levelCombinations = new List<roundPair>();
      
    private void Awake()//singleton
    {
        if(instance != null)
        {
            return;
        }
        instance = this;
        gameRoundsCompleted = 0;
        
        
    }

    private void Start()
    {
        //temporarily starting with an intermission for testing purposes
        levelCombinations = new List<roundPair>();
        addRound(new Intermission());
        startRound();
        
    }

    public void addRound(Round r)
    {
        nextRounds.Add(r);
    }

    public void onDeath(object sender, PlayerArgs e)
    {
        deadPlayers++;
        //Debug.log("$ONDEWATH" + deadPlayers.ToString() + " " + (totalPlayers - 1).ToString());
            if (deadPlayers > totalPlayers - 1)
            {
            endRound("All players died");
            deadPlayers = 0;
        }
       
        
    }

    public void startRound()
    {
        if(gameRoundsCompleted == 4)
        {
            //switch scene
            gameRoundsCompleted = 0;
            SceneManager.LoadScene(gameEndSceneName);
            return;
        }
        //Debug.log("$-------------");
        currentRoundSeconds = 0;//reset time of rounds
        currentRoundSecondsElapsed = 0;
        deadPlayers = 0;
        int toLoad = 0;
        int roundSeconds = 0;
        foreach(Round r in nextRounds)
        {
            r.load();
            toLoad += (int)r.getType();
            roundSeconds += r.getRoundTime();
            currentRounds.Add(r);
        }

        if (nextRoundsHaveIntermission())//if next round is intermission, go to intermission
        {
            //Debug.log("$START HAS INTERMISSION");
            sendPlayersToIntermission();
           
        }
        else
        {
            //Debug.log("$START HAS NO INTERMISSION");
            
           
            sendPlayersToLevel(loadLevel(toLoad));
        }
        
       //reset lists for next round
        
        currentRoundSeconds = roundSeconds;
        updateScreenClock();
        roundSeconds = 0;
        
        nextRounds.Clear();

        //subscribe to events
        EventManager.onPlayerFell += onDeath;
        EventManager.onSecondTickEvent += secondTick;//subscribe to the second ticking event
        EventManager.onRoundStart?.Invoke(null, System.EventArgs.Empty);//invoke the round start event for other scripts
        
    }

    public void endRound( string why)
    {
        
        //Debug.log("$WHY: " + why);
        EventManager.onSecondTickEvent -= secondTick;//unsubscribe from the second tick event (so the clock stops)
        EventManager.onPlayerFell -= onDeath;
        EventManager.onRoundEnd?.Invoke(null, System.EventArgs.Empty);
        
        //deadPlayers = 0;
        //remove children from the levelManager (destroys the level that was spawned in)
        for (int i = 0; i < GameManager.instance.levelManager.transform.childCount; i++) {
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
        
        //check if its intermission
        if (!currentRoundsHaveIntermission())
        {
            currentRounds.Clear();//to clear it before next rounds get loaded (but must be available to check for intermission above)
            //Debug.log("$END ROUND HAS NO INTERMISSION");
            gameRoundsCompleted++;
            addRound(new Intermission());
            startRound();
        }
        else
        {//start rounds
            currentRounds.Clear();
            //Debug.log("$END ROUND INTERMISSION");
            generateNextRoundLevels();
            startRound();
        }
    }

    private void generateNextRoundLevels()
    {

        if(levelCombinations.Count == 0)
        {
            //generate level combinations again
            List<roundType> possibleRounds = new List<roundType>();


            foreach (roundType r in System.Enum.GetValues(typeof(roundType)))//make a list of all roundTypes
            {
                if (r == roundType.NONE || r == roundType.INTERMISSION)
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
            foreach(roundPair roundPair in levelCombinations)
            {
                randomizedCombinations.Add(roundPair);
            }
            levelCombinations.Clear();
            while(randomizedCombinations.Count != 0)
            {
                int randomIndex = Random.Range(0, randomizedCombinations.Count);
                levelCombinations.Add(randomizedCombinations[randomIndex]);
                randomizedCombinations.RemoveAt(randomIndex);
            }
        }



        int index = gameRoundsCompleted % levelCombinations.Count;
        addRound(levelCombinations[index].getRoundOne());
        addRound(levelCombinations[index].getRoundTwo());
       
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
        return new Intermission();
    }

    public List<Transform> loadLevel(int number)
    {//instantiate from resources/load
        GameObject level = Instantiate(Resources.Load<GameObject>("Levels/" + number.ToString()));
        List<Transform> spawnPoints = new List<Transform>();
        Transform levelSpawnParent = level.transform.Find("SpawnPoints");
        for (int i = 0; i < 4; i++) {
            spawnPoints.Add(levelSpawnParent.GetChild(i));
        }
        level.transform.SetParent(GameManager.instance.levelManager.transform);
        level.transform.position = level.transform.parent.transform.position + level.transform.position;
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

    protected void sendPlayersToLevel(List<Transform> t)
    {
        //Debug.log("$SEND TO LEVEL");
        sendPlayersToLocation(t);
    }

    protected void sendPlayersToIntermission()
    {
        //Debug.log("$SEND TO INTERMISSION");
        sendPlayersToLocation(new List<Transform> { intermissionLocation });
    }

    protected void sendPlayersToLocation(List<Transform> t)
    {
        int index = 0;
        foreach(GameObject g in currentPlayers)//for (int i = 0; i < GameManager.instance.playerManager.transform.childCount; i++)
        {
            //Vector3 offset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            //GameManager.instance.playerManager.transform.GetChild(i).transform.position = teleportLocation + offset;
            g.transform.position = t[index%t.Count].position;// + offset;

            index++;
        }
    }
    public void secondTick(object sender, System.EventArgs e)//called every second
    {
        currentRoundSecondsElapsed++;
        updateScreenClock();
        
        ////Debug.log("SEC LEFT " +( currentRoundSeconds - currentRoundSecondsElapsed).ToString());
        if(currentRoundSeconds - currentRoundSecondsElapsed == 10)
        {
            EventManager.onTenSecondsBeforeRoundEndEvent?.Invoke(null, System.EventArgs.Empty);
        }
        EventManager.onRoundSecondTickEvent?.Invoke(null, new RoundTickArgs(currentRoundSecondsElapsed, currentRoundSeconds - currentRoundSecondsElapsed, currentRoundSeconds));
        if (currentRoundSecondsElapsed == currentRoundSeconds)
        {
            endRound("Time's up!");
        }
       
    }

   
    void updateScreenClock()//function to update the clock
    {
        int counter = currentRoundSeconds - currentRoundSecondsElapsed ;
        int minutes = 0;
        int seconds = 0;
        string extraSecondZero = "";
        while (counter> 59)
        {
            counter -= 60;
            minutes++;
        }
        seconds = counter;
        if(seconds < 10)
        {
            extraSecondZero = "0";
        }

         

        clockObject.text = minutes.ToString() + ":" + extraSecondZero + seconds.ToString();
    }
}

public struct roundPair{
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


