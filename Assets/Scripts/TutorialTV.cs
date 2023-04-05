using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialTV : MonoBehaviour
{

    public TMP_Text title;
    public TMP_Text subtitle;
    public DynamicUIComponent CR;
    public DynamicUIComponent RC;
    //public DynamicUIComponent LC;
    int timesEnded = 0;
    bool isInAnimation = false;
    // Start is called before the first frame update
    void Start()
    {
       // CR.onEnd += onEnd;
       // RC.onEnd += onEnd;
        //LC.onEnd += onEnd;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setTitleText(string text)
    {
        title.text = text;
    }
    public void setSubtitleText(string text)
    {
        subtitle.text = text;
    }

    public void onEnd(object sender, System.EventArgs e)
    {

        timesEnded++;
        if(timesEnded == 1)
        {
            //RL
            CR.StartToEnd(0.4f);
        }
        if(timesEnded == 2)
        {
            RC.StartToEnd(0.4f);
            isInAnimation = false;
            timesEnded = 0;
            //LC
        }
        if(timesEnded == 3)
        {
            isInAnimation = false;
            timesEnded = 0;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Bullet" && !isInAnimation)
        {
           // CR.StartToEnd(0.4f);
            //isInAnimation = true;
        }
    }
}
