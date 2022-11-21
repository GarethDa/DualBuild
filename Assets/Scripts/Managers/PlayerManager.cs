using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    protected int playersDied = 0;
    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    protected void checkLastOneStanding()
    {
        if (playersDied == GameManager.instance.playersConnected - 1)
        {
            RoundManager.instance.endRound();
        }
    }

    public void playerDied(GameObject p)
    {
        playersDied++;
        EventManager.onPlayerFell?.Invoke(null, new PlayerArgs(p));
        checkLastOneStanding();
    }

    public int getPlayersDied()
    {
        return playersDied;
    }

    public void resetPlayerDeaths()
    {
        playersDied = 0;
    }

}
