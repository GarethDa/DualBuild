using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : MonoBehaviour
{

    private bool isHeld = false;
    private bool isThrown = false;
    private Vector3 startPosition;
    private GameObject playerCam;
    private List<GameObject> playerObjects = new List<GameObject>();
    [SerializeField] int hitForce = 500;
    [SerializeField] float throwLife = 3f;
    [SerializeField] Material thrownMat;

    [SerializeField] private Color notThrownColour;
    [SerializeField] private Color thrownColour;

    Material origMat;

    float currentThrowLife = 0f;

    void Start()
    {
        //Find the player camera and player objects
        playerCam = GameObject.Find("Main Camera");

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            playerObjects.Add(obj);

        origMat = gameObject.GetComponent<MeshRenderer>().material;

        gameObject.GetComponent<TrailRenderer>().startColor = notThrownColour + new Color(0, 0, 0, 1);

        startPosition = transform.position;

        //gameObject.GetComponent<MeshRenderer>().material.color = notThrownColour;
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
                gameObject.GetComponent<TrailRenderer>().startColor = notThrownColour + new Color(0, 0, 0, 1);
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
            collision.rigidbody.AddExplosionForce(hitForce, collision.contacts[0].point, 10, 50);
            //Debug.Log("Deez");
        }
        if (isThrown && collision.gameObject.tag == "NetworkedPlayer")
        {
            //collision.rigidbody.AddExplosionForce(hitForce, collision.contacts[0].point, 10, 50);
            Vector3 offset = collision.contacts[0].point - collision.transform.position;
            collision.gameObject.GetComponent<NetworkedPhysics>().sendCurrentDataExplosion(offset, hitForce,50,10);
            
            //Debug.Log("Deez");
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
                Debug.Log(other.transform.parent.Find("Orientation").name);
                other.transform.parent.Find("Orientation").GetComponent<tutorialShower>().hideTutorial();
            }
        }
    }

    public void SetIsHeld(bool held)
    {
        isHeld = held;

        if (held)
        {
            gameObject.GetComponent<MeshRenderer>().material = thrownMat;
            gameObject.GetComponent<TrailRenderer>().startColor = thrownColour + new Color(0, 0, 0, 1);
        }    
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
            gameObject.GetComponent<TrailRenderer>().startColor = thrownColour + new Color(0, 0, 0, 1);
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

    public void ResetBall()
    {
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        transform.position = startPosition;
    }
}
