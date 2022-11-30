using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperRound : Round
{
    public BumperRound()
    {
        hasMap = false;
        roundTime = 10;
        mapPrefabName = "Spinner";
        type = roundType.BUMPER;
    }

    protected override void setWinCondition()
    {
        
    }

    public override void unload()
    {
        
    }
}
