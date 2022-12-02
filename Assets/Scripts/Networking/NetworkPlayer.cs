using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class NetworkPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    Vector3 accPos = Vector3.zero;
    Quaternion accRot = Quaternion.identity;
    Quaternion prevRot;
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animation here");
        }

        photonView.RPC("OnAim", RpcTarget.All);
    }

    // Update is called once per frame
    void Update()
    {
        prevRot = transform.Find("PlayerObj").rotation;
        if (photonView.IsMine)
        {
            //nothing
            Debug.Log(prevRot);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, accPos, 0.1f);
            //transform.rotation = Quaternion.Lerp(transform.rotation, accRot, 0.1f);
            prevRot = Quaternion.Lerp(prevRot, accRot, 0.1f);
            
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        //the player YOU control, send actual pos to network
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(prevRot);
            stream.SendNext(animator.GetBool("isGrounded"));
            stream.SendNext(animator.GetBool("isRunning"));
            stream.SendNext(animator.GetBool("isAiming"));
            stream.SendNext(animator.GetBool("Throw"));
            stream.SendNext(animator.GetBool("hasBall"));
        }

        //other player someone ELSE controls, send that pos to network
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            prevRot = (Quaternion)stream.ReceiveNext();
            animator.SetBool("isGrounded", (bool)stream.ReceiveNext());
            animator.SetBool("isRunning", (bool)stream.ReceiveNext());
            animator.SetBool("isAiming", (bool)stream.ReceiveNext());
            animator.SetBool("Throw", (bool)stream.ReceiveNext());
            animator.SetBool("hasBall", (bool)stream.ReceiveNext());
        }

    }
}
