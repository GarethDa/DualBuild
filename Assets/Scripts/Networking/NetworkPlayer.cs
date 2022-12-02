using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class NetworkPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    Vector3 accPos = Vector3.zero;
    Quaternion accRot = Quaternion.identity;
    Quaternion rot;

    Animator animator;
    void Start()
    {
        rot = transform.Find("PlayerObj").rotation;
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animation here");
        }
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
            //transform.rotation = Quaternion.Lerp(transform.rotation, accRot, 0.1f);
            rot = Quaternion.Lerp(rot, accRot, 0.1f);
            
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        //the player YOU control, send actual pos to network
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(rot);
            stream.SendNext(animator.GetBool("isGrounded"));
            stream.SendNext(animator.GetBool("isRunning"));
        }

        //other player someone ELSE controls, send that pos to network
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            rot = (Quaternion)stream.ReceiveNext();
            animator.SetBool("isGrounded", (bool)stream.ReceiveNext());
            animator.SetBool("isRunning", (bool)stream.ReceiveNext());
        }

    }
}
