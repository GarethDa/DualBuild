using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBehaviour : MonoBehaviour
{

    private bool isHeld = false;
    private bool isThrown = false;
    private GameObject playerCam;
    private List<GameObject> playerObject = new List<GameObject>();
    [SerializeField] [Range(1f, 1000f)] int bombForce = 5000;
    [SerializeField] [Range(1f, 100f)] int bombRadius = 500;

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

    void OnCollisionEnter(Collision collision)
    {
        /*
        //if the ball hits a player after being thrown
        if (isThrown && collision.gameObject.tag == "Player")
        {
            collision.rigidbody.AddExplosionForce(hitForce, collision.contacts[0].point, 5);
            Debug.Log("Hit Player");
            ParticleManager.instance.PlayEffect(transform.position, "RedParticles");
        }

        //if the ball hits the ground while not being held
        if (!isHeld && collision.gameObject.layer == LayerMask.NameToLayer("FloorLayer"))
        {
            isThrown = false;
            ParticleManager.instance.PlayEffect(transform.position, "RedParticles");
        }
        */
        if (isThrown && !isHeld)
        {
            ParticleManager.instance.PlayEffect(transform.position, "RedParticles");

            Collider[] colliders = Physics.OverlapSphere(transform.position, bombRadius);

            foreach (Collider hit in colliders)
            {

                if (hit.tag == "Player")
                {
                    Rigidbody rb = hit.GetComponentInParent<Rigidbody>();

                    if (rb != null)
                    {
                        Debug.Log("Bomb Hit Player");
                        rb.AddExplosionForce(bombForce, transform.position, bombRadius, 3f, ForceMode.Force);
                        gameObject.SetActive(false);
                    }
                }
                else
                {
                    Rigidbody rb = hit.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        Debug.Log("Bomb Hit " + hit.name);
                        rb.AddExplosionForce(bombForce, transform.position, bombRadius, 10f, ForceMode.Impulse);
                        gameObject.SetActive(false);
                    }

                }
            }
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

    public void SetIsHeld(bool held)
    {
        isHeld = held;
    }

    public void SetIsThrown(bool thrown)
    {
        isThrown = thrown;
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
