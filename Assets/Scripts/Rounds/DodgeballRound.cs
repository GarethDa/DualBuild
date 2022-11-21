using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeballRound : Round
{
    public DodgeballRound()
    {
        hasMap = false;
        roundTime = 10;
        mapPrefabName = "Dodgeball";
        type = roundType.DODGEBALL;
    }

    public override void onTenSecondsBefore(object sender, System.EventArgs e)
    {
        EventManager.onOffAirShowEvent?.Invoke(null, System.EventArgs.Empty);
    }

    protected override void Load()
    {
        //none
        EventManager.onPlayerFell += playerFell;
        EventManager.onTenSecondsBeforeRoundEndEvent += onTenSecondsBefore;

    }

    public void playerFell(object sender, PlayerArgs e)
    {
        //do nothing for now
       
        e.player.transform.position = GameManager.instance.deathZone.transform.position;
        EventManager.onPlayerDeath?.Invoke(sender, e);
    }


    public override void unload()
    {
        EventManager.onPlayerFell -= playerFell;
        EventManager.onOffAirHideEvent?.Invoke(null, System.EventArgs.Empty);
    }
}
