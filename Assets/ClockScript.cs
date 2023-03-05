using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClockScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.onRoundSecondTickEvent += updateClock;

        GetComponent<TextMeshProUGUI>().color = new Color(0f, 0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateClock(object sender, RoundTickArgs e)
    {
        updateScreenClock(e.totalSeconds, e.secondsElapsed);
    }

    void updateScreenClock(int current, int elapsed)//function to update the clock
    {
        GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 1f);
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

        GetComponent<TextMeshProUGUI>().text = minutes.ToString() + ":" + extraSecondZero + seconds.ToString();
    }
}
