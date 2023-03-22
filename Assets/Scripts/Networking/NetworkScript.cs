using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetworkScript : MonoBehaviour
{
    public bool isHost = false;
    public float tolerance = 1f;
    public bool sendAnyways = false;
    public bool sendDataPeriodically = true;
    protected abstract void applyData();//for the receiving end (playersingle > PLAYERNETWORKED)

    protected abstract void sendData();//for the sending end (PLAYERSINGLE > playernetworked)

    public abstract void setData(object d);

    public virtual void frameAdjustment()
    {

    }

    public int framesToSkipSending = 0;

    public void Start()
    {
        if (!GameManager.instance.isNetworked)
        {
            Destroy(this);
        }
    }
    public void Update()
    {
        if (!sendDataPeriodically)
        {
            return;
        }
        frameAdjustment();
        if (framesToSkipSending > 0)
        {
           
            applyData();
            framesToSkipSending--;
            return;
        }
        if (!isHost)
        {
            if (!sendAnyways)
            {
                return;
            }
            
        }
        
        sendData();
        
    }
}
