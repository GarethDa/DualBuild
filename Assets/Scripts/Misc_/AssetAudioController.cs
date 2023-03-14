using GameAudioScriptingEssentials;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetAudioController : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        //Dodgeball SFX
        //Adjust how fast the dodgeball can be for the sound to be played
        if (gameObject.CompareTag("Bullet") && GetComponent<Rigidbody>().velocity.magnitude > 10f)
        {
            if (collision.gameObject.tag == "Player" || collision.gameObject.layer == 6)
            {
                GetComponent<AudioClipRandomizer>().PlaySFX();
                //Make AudioManager, instance.PlayAudio?
            }
            
        }
        
        //Pachinko SFX
        if (gameObject.CompareTag("Pachinko") && GetComponent<Rigidbody>().velocity.magnitude > 5f)
        {
            if (collision.gameObject.tag == "Player" || collision.gameObject.layer == 6)
            {
                GetComponent<AudioClipRandomizer>().PlaySFX();
            }
        }

    }

    public void BombSFX()
    {
        GetComponent<AudioDestroy>().setCalled(true);
        GetComponent<AudioClipRandomizer>().PlaySFX();
    }
}
