using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class NetworkedPrefab : MonoBehaviourPunCallbacks
{
    public GameObject Prefab;
    public string Path;
    Vector3 Pos;

    private void Start()
    {
        ObjectInstatiate();
    }

    void ObjectInstatiate()
    {
        PhotonNetwork.Instantiate("Projectiles/Dodgeball", Pos, Quaternion.identity);
    }
}
