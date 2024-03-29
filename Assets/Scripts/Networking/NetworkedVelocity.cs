using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedVelocity : NetworkScript
{
    public Vector3 newVelocity;//non normalized velocity
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
        transform.position += (newVelocity * Time.deltaTime);
        //gameObject.GetComponent<NetworkedPosition>().setData(transform.position + (newVelocity * Time.deltaTime)) ;
    }

    public override void frameAdjustment()
    {
        if (isHost)
            return;

        applyData();
    }

    protected override void sendData()
    {
        //Debug.Log("sending velocity");
        newVelocity = GetComponent<Rigidbody>().velocity;
        string data = JsonUtility.ToJson(newVelocity) + "|" + gameObject.GetInstanceID();
        NetworkManager.instance.queueTCPInstruction(this, NetworkManager.instance.getInstruction(InstructionType.VELOCITY_CHANGE, data));//.sendUDPMessage();

    }
}
