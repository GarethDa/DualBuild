using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallBehaviour : MonoBehaviour
{

    private bool isHeld = false;
    private bool isThrown = false;
    private GameObject playerCam;
    private List<GameObject> playerObject = new List<GameObject>();
    [SerializeField] int hitForce = 500;

    void Start()
    {
        //Find the player camera and player objects
        playerCam = GameObject.Find("Main Camera");

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            playerObject.Add(obj);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    [PunRPC]
    void OnCollisionEnter(Collision collision)
    {
        //if the ball hits a player after being thrown
        if (isThrown && collision.gameObject.tag == "Player")
        {
            collision.rigidbody.AddExplosionForce(hitForce, collision.contacts[0].point, 5);
            Debug.Log("Deez");
        }

        //if the ball hits the ground while not being held
        if (!isHeld && collision.gameObject.layer == LayerMask.NameToLayer("FloorLayer"))
        {
            isThrown = false;
        }

        //If the player touches a ball that isn't being held, and has not been thrown, and the player isn't already holding a ball
        if (!isHeld && !isThrown && collision.gameObject.tag == "Player")
        {

            if (collision.gameObject.GetComponent<CharacterAiming>() != null && !collision.gameObject.GetComponent<CharacterAiming>().IsHoldingProj())
            {
                //Update the ball to be held
                isHeld = true;

                collision.gameObject.GetComponent<CharacterAiming>().SetProjectile(this.gameObject);
            }
        }
    }

    [PunRPC]
    public void SetIsHeld(bool held)
    {
        isHeld = held;
    }

    [PunRPC]
    public void SetIsThrown(bool thrown)
    {
        isThrown = thrown;
    }

    [PunRPC]
    public bool GetIsHeld()
    {
        return isHeld;
    }

    [PunRPC]
    public bool GetIsThrown()
    {
        return isThrown;
    }
}