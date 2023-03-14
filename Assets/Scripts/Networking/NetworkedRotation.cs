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
        if (!(d is Vector4))
        {
            return;
        }
        Vector4 dv = (Vector4)d;
        Quaternion rot = new Quaternion(dv.x, dv.y, dv.z, dv.w);
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
            Quaternion q = transform.rotation;
            Vector4 sending = new Vector4(q.x, q.y, q.z, q.w);
            string data = JsonUtility.ToJson(sending) + "|" + gameObject.GetInstanceID();
            NetworkManager.instance.queueTCPInstruction(this, NetworkManager.instance.getInstruction(InstructionType.ROTATION_CHANGE, data));//.sendUDPMessage();

        }


    }
}
