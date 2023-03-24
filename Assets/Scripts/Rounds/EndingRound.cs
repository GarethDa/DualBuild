using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingRound : Round
{
    public EndingRound()
    {
        hasMap = false;
        roundTime = 11;
        mapPrefabName = "Spinner";
        tutorialText = "THE END";

        type = roundType.ENDING;
    }

    public override void onTenSecondsBefore(object sender, EventArgs e)
    {
        //do nothing
    }

    public override void unload()
    {
        SceneManager.LoadScene("newMainMenu");
    }

    protected override void Load()
    {
        
        //throw new NotImplementedException();
    }

  
}
