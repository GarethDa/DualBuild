using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance = null;//singleton
    List<Round> nextRounds = new List<Round>();//rounds that are being played
    List<Round> currentRounds = new List<Round>();//rounds that get queued for next time
    int currentRoundSeconds = 0;//seconds the current round should last
    int currentRoundSecondsElapsed = 0;//current elapsed time

    public TextMeshProUGUI clockObject;//clock text on canvas
    public Transform intermissionLocation;//where to spawn players for intermission
    public Transform levelLocation;//where to spawn players when a round starts
    public GameObject deathLocation;//where players go when they die (AKA purgatory)


    private void Awake()//singleton
    {
        if(instance != null)
        {
            return;
        }
        instance = this;

        
        
    }

    private void Start()
    {
        //temporarily starting with an intermission for testing purposes
        addRound(new Intermission());
        startRound();
        
    }

    public void addRound(Round r)
    {
        nextRounds.Add(r);
    }

    public void startRound()
    {   
        currentRoundSeconds = 0;//reset time of rounds
 
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
            sendPlayersToIntermission();
            
        }
        else
        {
            loadLevel(toLoad);//load the level needed
            sendPlayersToLevel();
        }
        
       //reset lists for next round
        currentRoundSecondsElapsed = 0;
        currentRoundSeconds = roundSeconds;
        updateScreenClock();
        roundSeconds = 0;
        
        nextRounds.Clear();

        //subscribe to events
        EventManager.onSecondTickEvent += secondTick;//subscribe to the second ticking event
        EventManager.onRoundStart?.Invoke(null, System.EventArgs.Empty);//invoke the round start event for other scripts
    }

    public void endRound()
    {
        EventManager.onSecondTickEvent -= secondTick;//unsubscribe from the second tick event (so the clock stops)
        EventManager.onRoundEnd?.Invoke(null, System.EventArgs.Empty);

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
            addRound(new Intermission());
            startRound();
        }
        else
        {//start rounds
            currentRounds.Clear();
            generateNextRoundLevels();
            startRound();
        }
    }

    private void generateNextRoundLevels()
    {
        List<roundType> playingRounds = new List<roundType>();
        List<roundType> possibleRounds = new List<roundType>();
        int levelsPerRound = 2;
           
        foreach(roundType r in System.Enum.GetValues(typeof(roundType)))//make a list of all roundTypes
        {
            if(r == roundType.NONE || r == roundType.INTERMISSION)
            {
                continue;
            }
            possibleRounds.Add(r);
        }
        for (int i = 0; i < levelsPerRound; i++)//pop a random element into a new list levelsPerRound amount of times
        {

            int randomIndex = Random.Range(0, possibleRounds.Count);       
            playingRounds.Add(possibleRounds[randomIndex]);
            possibleRounds.RemoveAt(randomIndex);
        }

        
       //queue rounds
        foreach (roundType r in playingRounds)
        {
            
            if (r == roundType.BUMPER)
            {
                addRound(new BumperRound());
            }
            if (r == roundType.DODGEBALL)
            {
                addRound(new DodgeballRound());
            }
            if (r == roundType.PACHINKO_BALL)
            {
                addRound(new PachinkoRound());
            }
            if (r == roundType.FALLING_PLATFORMS)
            {
                addRound(new FallingPlatformRound());
            }
        }
    }

    public void loadLevel(int number)
    {//instantiate from resources/load
        GameObject level = Instantiate(Resources.Load<GameObject>("Levels/" + number.ToString()));
        level.transform.SetParent(GameManager.instance.levelManager.transform);
        level.transform.position = level.transform.parent.transform.position + level.transform.position;
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

    protected void sendPlayersToLevel()
    {
        sendPlayersToLocation(levelLocation.position);
    }

    protected void sendPlayersToIntermission()
    {
        
        sendPlayersToLocation(intermissionLocation.position);
    }

    protected void sendPlayersToLocation(Vector3 teleportLocation)
    {
        for (int i = 0; i < GameManager.instance.playerManager.transform.childCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            GameManager.instance.playerManager.transform.GetChild(i).transform.position = teleportLocation + offset;
        }
    }
    public void secondTick(object sender, System.EventArgs e)//called every second
    {
        currentRoundSecondsElapsed++;
        updateScreenClock();

        //Debug.Log("SEC LEFT " +( currentRoundSeconds - currentRoundSecondsElapsed).ToString());
        if(currentRoundSeconds - currentRoundSecondsElapsed == 10)
        {
            EventManager.onTenSecondsBeforeRoundEndEvent?.Invoke(null, System.EventArgs.Empty);
        }
        EventManager.onRoundSecondTickEvent?.Invoke(null, new RoundTickArgs(currentRoundSecondsElapsed, currentRoundSeconds - currentRoundSecondsElapsed, currentRoundSeconds));
        if (currentRoundSecondsElapsed == currentRoundSeconds)
        {
            endRound();
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



