
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intermission : Round
{
    public Intermission()
    {
        type = roundType.INTERMISSION;
        roundTime = 1000;
        hasMap = true;
    }

    public override void unload()
    {
        //didnt load anything

        EventManager.onOnAirHideEvent?.Invoke(null, System.EventArgs.Empty);
        EventManager.onTenSecondsBeforeRoundEndEvent -= onTenSecondsBefore;
        playerFallScript.instance.resetFallenPlayers();
    }

    public override void onTenSecondsBefore(object sender, System.EventArgs e)
    {
        Debug.Log("INTERMISSION INVOKED");
        EventManager.onOnAirShowEvent?.Invoke(null, System.EventArgs.Empty);
    }

    protected override void Load()
    {
        //do nothing, wait for time to expire
        EventManager.onTenSecondsBeforeRoundEndEvent += onTenSecondsBefore;

    }
}
