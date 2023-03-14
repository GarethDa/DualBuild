using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedRotation : NetworkScript
{
    Quaternion oldRoation = Quaternion.identity;
    Quaternion difference = Quaternion.identity;
    public Transform affectingTransform;
    float t = 0f;
    
    public override void setData(object d)
    {
        if (!(d is Vector4))
        {
            return;
        }
        Vector4 dv = (Vector4)d;
        Quaternion rot = new Quaternion(dv.x, dv.y, dv.z, dv.w);
        difference = affectingTransform.rotation * Quaternion.Inverse(rot);
        affectingTransform.rotation = rot;
        
        oldRoation = rot;
        framesToSkipSending = 1;
        t = 0;
    }

    protected override void applyData()
    {
        if (isHost)
        {
            return;
        }
        affectingTransform.rotation = oldRoation;
    }
    public override void frameAdjustment()//https://forum.unity.com/threads/get-the-difference-between-two-quaternions-and-add-it-to-another-quaternion.513187/
    {
        if (isHost)
        {
            return;
        }
        Quaternion perFrame = Quaternion.Slerp(Quaternion.identity, difference, Time.deltaTime);
        affectingTransform.rotation = affectingTransform.rotation * perFrame;
    }
    protected override void sendData()
    {
        tolerance = 0.1f;
        //if(1 - Mathf.Abs(Quaternion.Dot(oldRoation, affectingTransform.rotation)) < tolerance)
        {
            //update position
            oldRoation = affectingTransform.rotation;
            //lastKnownRotation = affectingTransform.rotation;
            Quaternion q = affectingTransform.rotation;
            Vector4 sending = new Vector4(q.x, q.y, q.z, q.w);
            string data = JsonUtility.ToJson(sending) + "|" + gameObject.GetInstanceID();
            NetworkManager.instance.queueTCPInstruction(this, NetworkManager.instance.getInstruction(InstructionType.ROTATION_CHANGE, data));//.sendUDPMessage();

        }


    }
}
