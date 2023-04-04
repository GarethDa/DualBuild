using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyBoundaryScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Player")
        {
            return;
        }
        //Debug.Log("ENTER");
        //check if its networked and send it to the server otherwise
        RoundManager.instance.onPlayerEnterReadyZone();
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player")
        {
            return;
        }
        //Debug.Log("LEAVE");
        RoundManager.instance.onPlayerExitReadyZone();
    }
}
