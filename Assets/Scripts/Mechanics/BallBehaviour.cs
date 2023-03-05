using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : MonoBehaviour
{

    private bool isHeld = false;
    private bool isThrown = false;
    private GameObject playerCam;
    private List<GameObject> playerObjects = new List<GameObject>();
    [SerializeField] int hitForce = 500;
    [SerializeField] float throwLife = 3f;
    [SerializeField] Material thrownMat;
    Material origMat;

    float currentThrowLife = 0f;

    void Start()
    {
        //Find the player camera and player objects
        playerCam = GameObject.Find("Main Camera");

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            playerObjects.Add(obj);

        origMat = gameObject.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponent<Rigidbody>().velocity.magnitude < 5f && !gameObject.GetComponent<Rigidbody>().isKinematic)
        {
            gameObject.GetComponent<TrailRenderer>().emitting = false;
        }

        else
            gameObject.GetComponent<TrailRenderer>().emitting = true;

        //If the ball is in thrown mode
        if (isThrown)
        {
            //Update the thrown timer
            currentThrowLife += Time.deltaTime;

            //If we reached the end of the thrown period
            if (currentThrowLife >= throwLife)
            {
                //Debug.Log("Ready for pickup");
                //Switch back to normal material
                gameObject.GetComponent<MeshRenderer>().material = origMat;
                isThrown = false;

                currentThrowLife = 0f;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //if the ball hits a player after being thrown
        if (isThrown && collision.gameObject.tag == "Player")
        {
            collision.rigidbody.AddExplosionForce(hitForce, collision.contacts[0].point, 5, 20);
            Debug.Log("Deez");
        }

        /*
        //if the ball hits the ground while not being held
        if (!isHeld && collision.gameObject.layer == LayerMask.NameToLayer("FloorLayer"))
        {
            isThrown = false;
        }
        */

        /*
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
        */
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isHeld && !isThrown && other.gameObject.tag == "Player")
        {
            //Debug.Log("here");
            if (!other.transform.parent.GetComponent<CharacterAiming>().IsHoldingProj())
                //(other.gameObject.GetComponent<CharacterAiming>() != null && !other.gameObject.GetComponent<CharacterAiming>().IsHoldingProj())
            {
                //Debug.Log("Here");
                //Update the ball to be held
                isHeld = true;

                other.transform.parent.GetComponent<CharacterAiming>().SetProjectile(this.gameObject);
            }
        }
    }

    public void SetIsHeld(bool held)
    {
        isHeld = held;
    }

    public void SetIsThrown(bool thrown)
    {
        isThrown = thrown;

        //If we just threw the ball
        if (isThrown)
        {
            //Debug.Log("thrown");
            currentThrowLife = 0f;
            //Change to the thrown material
            gameObject.GetComponent<MeshRenderer>().material = thrownMat;
        }
    }

    public bool GetIsHeld()
    {
        return isHeld;
    }

    public bool GetIsThrown()
    {
        return isThrown;
    }
}
