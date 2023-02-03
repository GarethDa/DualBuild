using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitParticleOnHit : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player" || collision.gameObject.tag == "Bullet" || collision.gameObject.layer == 6)
        {
           if(gameObject.GetComponent<Rigidbody>().velocity.magnitude > 31)
            {
                ParticleManager.instance.PlayEffect(collision.contacts[0].point, "Hit");
            }
            else
            {
                ParticleManager.instance.PlayEffect(collision.contacts[0].point, "HitSmall");
            }
               
               
            
            
        }
       
    }
}
