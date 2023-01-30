using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class EventManager 
{
    //list of all events that will be a part of the game
    public static EventHandler onSecondTickEvent;
    public static EventHandler<RoundTickArgs> onRoundSecondTickEvent;
    public static EventHandler onTenSecondsBeforeRoundEndEvent;
    public static EventHandler onOnAirShowEvent;
    public static EventHandler onOnAirHideEvent;
    public static EventHandler onOffAirShowEvent;
    public static EventHandler onOffAirHideEvent;
    public static EventHandler<RoundArgs> onRoundEnd;
    public static EventHandler<RoundArgs> onRoundStart;
    public static EventHandler<PlayerArgs> onPlayerDeath;//TODO: remove player from the pool of players that can get rewards in some new manager class we have to make
    public static EventHandler<PlayerArgs> onPlayerFell;
    public static EventHandler<CollectableArgs> onPlayerCollect;
    public static EventHandler onPlayerUsePowerup;

    
}

public class RoundArgs : EventArgs
{
    roundType[] rounds = new roundType[2];
    public RoundArgs(roundType[] typesInRound)
    {
        rounds = typesInRound;
    }

    public roundType getRound(int index)
    {
        return rounds[index];
    }
}

public class RoundTickArgs : EventArgs
{
    public int secondsElapsed;
    public int secondsLeft;
    public int totalSeconds;
    

    public RoundTickArgs(int elapsed, int left, int total)
    {
        secondsElapsed = elapsed;
        secondsLeft = left;
        totalSeconds = total;
        
    }
}

//argument example
public class CollectableArgs : EventArgs
{
    public Collectable collectableObject;
    //TODO: add player to this
    public CollectableArgs(Collectable c)
    {
        collectableObject = c;
    }
}

public class PlayerArgs :EventArgs
{
    public GameObject player;

    public PlayerArgs(GameObject p)
    {
        player = p;
    }
}
