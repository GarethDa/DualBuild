using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    Vector3 accPos = Vector3.zero;
    Quaternion accRot = Quaternion.identity;
 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            //nothing
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, accPos, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, accRot, 0.1f);
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        //the player YOU control, send actual pos to network
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }

        //other player someone ELSE controls, send that pos to network
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }

    }
}
