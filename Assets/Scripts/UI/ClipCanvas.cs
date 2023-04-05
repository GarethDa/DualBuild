using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipCanvas : MonoBehaviour
{

    public DynamicUIComponent top;
    public static ClipCanvas instance;
    // Start is called before the first frame update
    void Start()
    {
       if(instance == null)
        {
            instance = this;
        }
        top.onEnd += playSound;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playSound(object sender, System.EventArgs e)
    {
        RoundManager.instance.GetComponent<AudioManager>().playActionCue();
        Debug.Log("SOUND");
    }

    public void clip()
    {
        GetComponent<DynamicUIComponent>().StartToEnd(0.4f);
        top.repeatEnd = true;
        GetComponent<DynamicUIComponent>().repeatEnd = true;
        //RoundManager.instance.GetComponent<AudioManager>().playActionCue();
        top.StartToEnd(0.15f);
        RoundManager.instance.updateScreenClock();
    }
}
