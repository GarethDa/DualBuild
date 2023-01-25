using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SlidingTVUI : DynamicUIComponent
{
    public bool isOnAir = false;
    public TextMeshProUGUI clockObject;//clock text on canvas
    public override void onStart()
    {
        if (isOnAir)
        {
            EventManager.onOnAirShowEvent += showEase;
            EventManager.onOnAirHideEvent += hideEase;
        }
        else
        {
            EventManager.onOffAirShowEvent += showEase;
            EventManager.onOffAirHideEvent += hideEase;
        }
        
        EventManager.onRoundSecondTickEvent += updateClock;
        base.onStart();
    }

    public void showEase(object sender, System.EventArgs e)
    {
        Debug.Log("UI ELEMENT SHOW " + "ISONAIR: " + isOnAir.ToString());
        easeIn(UIManager.instance.UIOnScreen, 3f, UIManager.instance.UIOffScreen);
    }

    public void hideEase(object sender, System.EventArgs e)
    {
        Debug.Log("UI ELEMENT HIDE " + "ISONAIR: " + isOnAir.ToString());

        easeIn(UIManager.instance.UIOffScreen, 0.5f, UIManager.instance.UIOnScreen);
    }

    public void updateClock(object sender, RoundTickArgs e)
    {
        updateScreenClock(e.totalSeconds, e.secondsElapsed);
    }

    void updateScreenClock(int current, int elapsed)//function to update the clock
    {
        int counter = current - elapsed;
        int minutes = 0;
        int seconds = 0;
        string extraSecondZero = "";
        while (counter > 59)
        {
            counter -= 60;
            minutes++;
        }
        seconds = counter;
        if (seconds < 10)
        {
            extraSecondZero = "0";
        }



        clockObject.text = minutes.ToString() + ":" + extraSecondZero + seconds.ToString();
    }
}
