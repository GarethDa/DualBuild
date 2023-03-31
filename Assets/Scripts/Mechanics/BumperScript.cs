using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperScript : MonoBehaviour
{
    [SerializeField] string ignoreTag = "Environment";
    [SerializeField] float _explosionForce = 50f;
    [SerializeField] float velocityFactor = 100f;
    [SerializeField] float _upwardForce = 150f;

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Collision!");
        if(collision.transform.tag != ignoreTag && collision.transform.tag == "Player")
        {
            //Debug.Log("BOOM!");
            Rigidbody otherRB = collision.rigidbody;

            if (collision.gameObject.GetComponentInParent<TpMovement>() != null)
            {
                collision.gameObject.GetComponentInParent<TpMovement>().HitStun();
            }
            else
            {
                Debug.LogError("You fucked up something");
            }

            otherRB.AddExplosionForce(GetComponent<Rigidbody>().velocity.magnitude * velocityFactor + _explosionForce, transform.position, 1, _upwardForce);
        }
    }
}