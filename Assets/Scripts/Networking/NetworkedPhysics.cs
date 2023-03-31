using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPhysics : NetworkScript
//playernetworked will be the host this time
{
    NetworkPhysicsData currrentData;
    public override void setData(object d)
    {
       if(!(d is NetworkPhysicsData))
        {
            return;
        }

        currrentData = (NetworkPhysicsData)d;
        applyData();
    }

    protected override void applyData()
    {
        Debug.Log("Added FORCE");

        Rigidbody rb = GetComponent<Rigidbody>();
        if(currrentData.type == 4)//explosion force
        {
            rb.AddExplosionForce(currrentData.strength, transform.position + currrentData.contactOffset, currrentData.radius, currrentData.upStrength);
            return;
        }
        rb.AddForceAtPosition(currrentData.velocity*currrentData.strength, currrentData.contactOffset, (ForceMode)currrentData.type);
    }

    protected override void sendData()
    {
        string data = JsonUtility.ToJson(currrentData) + "|" + gameObject.GetInstanceID();
        NetworkManager.instance.queueTCPInstruction(this, NetworkManager.instance.getInstruction(InstructionType.APPLY_PHYSICS, data),false);//.sendUDPMessage();

    }

    public void sendCurrentDataNormal(Vector3 offset, Vector3 vel, float str, ForceMode mode)
    {
        
        currrentData = new NetworkPhysicsData(offset, vel, str, (int)mode);
        applyData();
        sendData();
    }

    public void sendCurrentDataExplosion(Vector3 offset, float str, float upStr, float rad)
    {
        currrentData = new NetworkPhysicsData(offset, str, upStr,rad, 4);
        applyData();
        sendData();
    }
   
}
[System.Serializable]
public class NetworkPhysicsData
{
    [SerializeField] public Vector3 contactOffset = Vector3.zero;//the place the force should be applied (in local space, offset from the objects origin (whatever we get from transform.position))
    [SerializeField] public float strength = -1;
    [SerializeField] public float upStrength = -1;
    [SerializeField] public float radius = -1;
    [SerializeField] public Vector3 velocity = Vector3.zero;
    [SerializeField] public int type = -1;

    public NetworkPhysicsData(Vector3 offset, Vector3 vel, float str, int mode)
    {
        type = mode;
        contactOffset = offset;
        velocity = vel;
        strength = str;

    }

    public NetworkPhysicsData(Vector3 offset, float str, float upStr,float rad, int mode)
    {
        type = mode;
        contactOffset = offset;
        strength = str;
        upStrength = upStr;
        radius = rad;
    }
}
