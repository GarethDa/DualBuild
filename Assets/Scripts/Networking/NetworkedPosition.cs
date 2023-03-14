using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPosition : NetworkScript
{
    Vector3 oldPosition = Vector3.zero;
    int objectsColliding = 0;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 6)
        {
            return;
        }
        sendAnyways = true;
        objectsColliding++;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 6)
        {
            return;
        }
        objectsColliding--;
        if(objectsColliding == 0)
        {
            sendAnyways = false;
        }
    }

    protected override void sendData()
    {
        if (Vector3.Distance(oldPosition,transform.position) > tolerance)
        {
            //update position
            oldPosition = transform.position;
            string data = JsonUtility.ToJson(transform.position) + "|" + gameObject.GetInstanceID();
            NetworkManager.instance.queueTCPInstruction(this, NetworkManager.instance.getInstruction(InstructionType.POSITION_CHANGE, data));//.sendUDPMessage();

        }
    }

    protected override void applyData()
    {
        oldPosition = transform.position;
    }
    public override void setData(object d)
    {
        if(!(d is Vector3))
        {
            return;
        }
        Vector3 pos = (Vector3)d;
        transform.position = pos;
        oldPosition = pos;
        framesToSkipSending = 1;
    }
   
}
