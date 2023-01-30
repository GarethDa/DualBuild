using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBehaviour : MonoBehaviour
{

   
    private bool isThrown = false;
    GameObject player;
    private List<Rigidbody> affectedObjects = new List<Rigidbody>();
    [SerializeField] [Range(1f, 1000f)] int bombForce = 850;
    [SerializeField] [Range(1f, 100f)] int bombRadius = 80;
    public SphereCollider outCollider;
    float radius = 0f;
    float timeThrown = 0f;
    float maxTimeThrown = 10f;

    void Start()
    {
       
       radius =  outCollider.bounds.size.x;
        

    }

    public void setPlayer(GameObject g)
    {
        player = g;
        player.GetComponent<CharacterAiming>().SetProjectile(this.gameObject);
    }

    public void setThrown(bool t)
    {
        isThrown = t;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isThrown)
        {
            return;
        }

        Debug.Log("HIT GROUND");
        //check for floor or player and explode
        if(collision.gameObject.tag == "Player" || collision.gameObject.layer == 6)
        {
            foreach (Rigidbody g in affectedObjects)
            {
                 Rigidbody rg = g.GetComponentInParent<Rigidbody>();
                //Vector3 toMove = transform.position - g.gameObject.transform.position;
                //Vector3 newForce = (radius - Vector3.Distance(transform.position, g.transform.position)) * toMove.normalized * bombForce;
                // g.AddForce(newForce, ForceMode.Impulse);
                rg.AddExplosionForce(bombForce, transform.position, radius, 3.0F);
                Debug.Log(rg.gameObject.name);

            }
            Destroy(gameObject);
        }
        
       
    }

    public void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.GetComponentInParent<Rigidbody>() != null)
        {
            affectedObjects.Add(other.gameObject.GetComponentInParent<Rigidbody>());
            Debug.Log("ADDED");
        }
    }

    public void OnTriggerExit(Collider other)
    {
        
        if (other.gameObject.GetComponent<Rigidbody>() != null)
        {
            if (!affectedObjects.Contains(other.gameObject.GetComponent<Rigidbody>()))
            {
                return;
            }
            affectedObjects.Remove(other.gameObject.GetComponent<Rigidbody>());
            Debug.Log("REMOVED");

        }
    }

    private void Update()
    {
        if (!isThrown)
        {
            return;
        }

        timeThrown += Time.deltaTime;
        if(timeThrown == maxTimeThrown)
        {
            Destroy(gameObject);
        }
    }

}
