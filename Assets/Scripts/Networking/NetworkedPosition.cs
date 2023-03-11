using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPosition : NetworkScript
{
    Vector3 oldPosition = Vector3.zero;
    public int framesToSkipSending = 0;
   //moving feedback loop. must fix

    public void Update()
    {
        if(framesToSkipSending > 0)
        {
            oldPosition = transform.position;
            framesToSkipSending--;
            return;
        }
        if(transform.position != oldPosition)
        {
            //update position
            oldPosition = transform.position;
            string data = JsonUtility.ToJson(transform.position) + "|" + gameObject.GetInstanceID();
            //Debug.Log("MOVED POSITION" + data);
            NetworkManager.instance.queueUDPInstruction(this,NetworkManager.instance.getInstruction(InstructionType.POSITION_CHANGE, data));//.sendUDPMessage();
        
        }
    }

    public void setPosition(Vector3 pos)
    {
        transform.position = pos;
        oldPosition = pos;
        framesToSkipSending = 1;
    }
}
