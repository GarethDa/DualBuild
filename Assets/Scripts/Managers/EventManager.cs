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
    public static EventHandler<StringArgs> onGetRoomKey;
    public static EventHandler<StringArgs> onNewPlayerJoined;
    public static EventHandler<StringArgs> onLeaderboardScore;
    public static EventHandler<IntArgs> onPlayerReady;

    public static void unsubscribeAll()
    {
        Delegate[] clientList;

        if (onSecondTickEvent != null)
        {

            clientList = onSecondTickEvent.GetInvocationList();
            foreach (var d in clientList)
            {
                onSecondTickEvent -= (d as EventHandler);
            }
        }

        if (onRoundSecondTickEvent != null)
        {

            clientList = onRoundSecondTickEvent.GetInvocationList();
            foreach (var d in clientList)
            {
                onRoundSecondTickEvent -= (d as EventHandler<RoundTickArgs>);
            }
        }


        if (onTenSecondsBeforeRoundEndEvent != null)
        {
            clientList = onTenSecondsBeforeRoundEndEvent.GetInvocationList();
            foreach (var d in clientList)
            {
                onTenSecondsBeforeRoundEndEvent -= (d as EventHandler);
            }
        }

        if (onOnAirShowEvent != null)
        {
            clientList = onOnAirShowEvent.GetInvocationList();
            foreach (var d in clientList)
            {
                onOnAirShowEvent -= (d as EventHandler);
            }
        }

        if (onOnAirHideEvent != null)
        {
            clientList = onOnAirHideEvent.GetInvocationList();
            foreach (var d in clientList)
            {
                onOnAirHideEvent -= (d as EventHandler);
            }
        }


        if (onOffAirShowEvent != null)
        {
            clientList = onOffAirShowEvent.GetInvocationList();
            foreach (var d in clientList)
            {
                onOffAirShowEvent -= (d as EventHandler);
            }
        }


        if (onOffAirHideEvent != null)
        {
            clientList = onOffAirHideEvent.GetInvocationList();
            foreach (var d in clientList)
            {
                onOffAirHideEvent -= (d as EventHandler);
            }
        }
        if (onRoundEnd != null)
        {
            clientList = onRoundEnd.GetInvocationList();
            foreach (var d in clientList)
            {
                onRoundEnd -= (d as EventHandler<RoundArgs>);
            }
        }


        if (onRoundStart != null)
        {
            clientList = onRoundStart.GetInvocationList();
            foreach (var d in clientList)
            {
                onRoundStart -= (d as EventHandler<RoundArgs>);
            }
        }


        if (onPlayerDeath != null)
        {
            clientList = onPlayerDeath.GetInvocationList();
            foreach (var d in clientList)
            {
                onPlayerDeath -= (d as EventHandler<PlayerArgs>);
            }
        }


        if (onPlayerFell != null)
        {
            clientList = onPlayerFell.GetInvocationList();
            foreach (var d in clientList)
            {
                onPlayerFell -= (d as EventHandler<PlayerArgs>);
            }
        }

        if (onLeaderboardScore != null)
        {
            clientList = onLeaderboardScore.GetInvocationList();
            foreach (var d in clientList)
            {
                onLeaderboardScore -= (d as EventHandler<StringArgs>);
            }
        }
        if (onNewPlayerJoined != null)
        {
            clientList = onNewPlayerJoined.GetInvocationList();
            foreach (var d in clientList)
            {
                onNewPlayerJoined -= (d as EventHandler<StringArgs>);
            }
        }
        if (onGetRoomKey != null)
        {
            clientList = onGetRoomKey.GetInvocationList();
            foreach (var d in clientList)
            {
                onGetRoomKey -= (d as EventHandler<StringArgs>);
            }
        }
        if (onPlayerUsePowerup != null)
        {
            clientList = onPlayerUsePowerup.GetInvocationList();
            foreach (var d in clientList)
            {
                onPlayerUsePowerup -= (d as EventHandler);
            }
        }
        if (onPlayerCollect != null)
        {
            clientList = onPlayerCollect.GetInvocationList();
            foreach (var d in clientList)
            {
                onPlayerCollect -= (d as EventHandler<CollectableArgs>);
            }
        }
        if (onPlayerReady != null)
        {
            clientList = onPlayerReady.GetInvocationList();
            foreach (var d in clientList)
            {
                onPlayerReady -= (d as EventHandler<IntArgs>);
            }
        }
    }
}

public class StringArgs : EventArgs
{
    public string str;
    public StringArgs(string s)
    {
        str = s;
    }
}
public class IntArgs : EventArgs
{
    public int i;
    public IntArgs(int inte)
    {
        i = inte;
    }
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


