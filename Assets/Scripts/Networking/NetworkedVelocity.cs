using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedVelocity : NetworkScript
{
    Vector3 newVelocity;//non normalized velocity
    public override void setData(object d)
    {
        if(!(d is Vector3))
        {
            return;
        }
        Vector3 vel = (Vector3)d;
        newVelocity = vel;
        framesToSkipSending = 1;
    }

    protected override void applyData()
    {
        gameObject.GetComponent<NetworkedPosition>().setData(transform.position + (newVelocity * Time.deltaTime)) ;
    }

    protected override void sendData()
    {
        
        string data = JsonUtility.ToJson(newVelocity) + "|" + gameObject.GetInstanceID();
        NetworkManager.instance.queueTCPInstruction(this, NetworkManager.instance.getInstruction(InstructionType.VELOCITY_CHANGE, data));//.sendUDPMessage();

    }
}
