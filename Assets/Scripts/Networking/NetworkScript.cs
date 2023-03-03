using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetworkScript : MonoBehaviour
{
    public abstract void sendToNetworkManager(byte[] data);

    public void applyData(string received)
    {
        //@ should be taken out by this point

    }

    private void Awake()
    {
        registerGameObject();
    }

    private void registerGameObject()
    {
        NetworkManager.instance.registerGameObject(gameObject);
    }
}
