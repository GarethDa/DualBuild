using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPowerUp : NetworkScript
{
    //STATUSES: 0 - pickup 1- use 2- effect 3- end
    public override void setData(object d)
    {
        List<float> list = (List<float>)d;
        //send message right now
        string data = JsonUtility.ToJson(list) + "|" + gameObject.GetInstanceID();
        NetworkManager.instance.queueTCPInstruction(this, NetworkManager.instance.getInstruction(InstructionType.POWERUP_USE, data));//.sendUDPMessage();

    }

    protected override void applyData()
    {
        //throw new System.NotImplementedException();
    }

    protected override void sendData()
    {
       // throw new System.NotImplementedException();
    }
}
