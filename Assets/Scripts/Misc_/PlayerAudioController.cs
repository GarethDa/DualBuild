using GameAudioScriptingEssentials;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    public float timeBetweenFootsteps = 0.5f;
    float soundCooldown;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        soundCooldown -= Time.deltaTime;
        if (GetComponent<TpMovement>().GetIsGrounded() == true && GetComponent<Rigidbody>().velocity.magnitude > 2f && soundCooldown < 0f)
        {
            GetComponent<AudioClipRandomizer>().PlaySFX();
            soundCooldown = timeBetweenFootsteps;
        }
    }

    public void Footsteps()
    {
        //empty for now
    }
}
