using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperScript : MonoBehaviour
{
    [SerializeField] string ignoreTag = "Environment";
    [SerializeField] float _explosionForce = 50f;
    [SerializeField] float _upwardForce = 75f;

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Collision!");
        if(collision.transform.tag != ignoreTag && collision.transform.tag == "Player")
        {
            //Debug.Log("BOOM!");
            Rigidbody otherRB = collision.rigidbody;
            otherRB.AddExplosionForce(otherRB.velocity.magnitude * _explosionForce, collision.contacts[0].point, 10, _upwardForce);
        }
    }
}
