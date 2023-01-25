using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewRound : Round
{
    public List<Round> nextRounds = new List<Round>();
    public PreviewRound(List<Round> adding)
    {
        hasMap = false;
        roundTime = 20;
        mapPrefabName = "NONE";
        tutorialText = "Tutorial";
        type = roundType.NONE;
        nextRounds = adding;
    }

    private void introduceTutorial(string text, int index)
    {
        RoundManager.instance.tutorialText[index].text = text;
        RoundManager.instance.tutorials[index].easeIn(RoundManager.instance.tutorialOnScreen[index], 1, RoundManager.instance.tutorialOffScreen[index]);
    }

    private void hideTutorial(int index)
    {
        RoundManager.instance.tutorials[index].easeIn(RoundManager.instance.tutorialOffScreen[index], 0.5f, RoundManager.instance.tutorialOnScreen[index]);
    }

    private void onRoundTick(object sender, RoundTickArgs e)
    {
        if(e.secondsLeft == 2)
        {
            hideTutorial(1);
        }
        if(e.secondsLeft == 1)
        {
            hideTutorial(0);
        }
        if(e.secondsElapsed == 1)
        {
            introduceTutorial(nextRounds[1].getTutorialText(),1);
        }
        if (e.secondsElapsed == 2)
        {
            introduceTutorial(nextRounds[0].getTutorialText(), 0);
        }
    }
    public override void onTenSecondsBefore(object sender, System.EventArgs e)
    {
        EventManager.onOffAirShowEvent?.Invoke(null, System.EventArgs.Empty);
    }

    protected override void Load()
    {
        //none
        EventManager.onRoundSecondTickEvent -= onRoundTick;
        EventManager.onRoundSecondTickEvent += onRoundTick;

        RoundManager.instance.camToDisable.enabled = false;
       RoundManager.instance.levelCam.enabled = true;
        RoundManager.instance.levelCam.GetComponent<PreviewCameraScript>().startMove();
        //EventManager.onTenSecondsBeforeRoundEndEvent -= onTenSecondsBefore;
       // EventManager.onTenSecondsBeforeRoundEndEvent += onTenSecondsBefore;

        TransformInterpolator levelCam = RoundManager.instance.levelCam.GetComponent<TransformInterpolator>();
        levelCam.setTransform(levelCam.edges[0].transform);

    }

    


    public override void unload()
    {
        EventManager.onRoundSecondTickEvent -= onRoundTick;
        RoundManager.instance.camToDisable.enabled = true;
        RoundManager.instance.levelCam.enabled = false;
        //EventManager.onTenSecondsBeforeRoundEndEvent -= onTenSecondsBefore;

    }
}
