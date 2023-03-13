using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetworkScript : MonoBehaviour
{
    public bool isHost = false;
    public float tolerance = 1f;
    protected abstract void applyData();//for the receiving end (playersingle > PLAYERNETWORKED)

    protected abstract void sendData();//for the sending end (PLAYERSINGLE > playernetworked)

    public abstract void setData(object d);

    public int framesToSkipSending = 0;
    public void Update()
    {
        if (framesToSkipSending > 0)
        {
           
            applyData();
            framesToSkipSending--;
            return;
        }
        if (!isHost)
        {
            return;
        }
        sendData();
        
    }
}
