using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedRotation : NetworkScript
{
    Quaternion oldRoation = Quaternion.identity;
    Quaternion lastKnownRotation = Quaternion.identity;
    float t = 0f;
    
    public override void setData(object d)
    {
        if (!(d is Quaternion))
        {
            return;
        }
        Quaternion rot = (Quaternion)d;
        transform.rotation = rot;
        oldRoation = rot;
        framesToSkipSending = 1;
        t = 0;
    }

    protected override void applyData()
    {
        transform.rotation = oldRoation;
    }
    public override void frameAdjustment()
    {
        applyData();
        return;
        t += Time.deltaTime * NetworkManager.instance.secondsBetweenUpdates;
        if(t > 1)
        {
            return;
        }
        transform.rotation = Quaternion.Slerp(lastKnownRotation, oldRoation, t);
    }
    protected override void sendData()
    {
        tolerance = 0.1f;
        if(1 - Mathf.Abs(Quaternion.Dot(oldRoation, transform.rotation)) < tolerance)
        {
            //update position
            oldRoation = transform.rotation;
            lastKnownRotation = transform.rotation;
            
            string data = JsonUtility.ToJson(transform.rotation) + "|" + gameObject.GetInstanceID();
            NetworkManager.instance.queueTCPInstruction(this, NetworkManager.instance.getInstruction(InstructionType.ROTATION_CHANGE, data));//.sendUDPMessage();

        }


    }
}
