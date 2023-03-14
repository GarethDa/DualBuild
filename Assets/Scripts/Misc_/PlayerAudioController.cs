using GameAudioScriptingEssentials;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    public float timeBetweenFootsteps = 0.5f;
    float soundCooldown;
    bool holding = false;
    AudioSource audioData;

    void Start()
    {
        audioData = GetComponent<AudioSource>();
    }

    void Update()
    {
        soundCooldown -= Time.deltaTime;
        if (GetComponent<TpMovement>().GetIsGrounded() == true && GetComponent<Rigidbody>().velocity.magnitude > 2f && soundCooldown < 0f)
        {
            GetComponent<AudioClipRandomizer>().PlaySFX();
            soundCooldown = timeBetweenFootsteps;
        }

        //When player grabs ball and continues to hold onto it
        if (GetComponent<CharacterAiming>().IsHoldingProj() == true && holding == false)
        {
            holding = true;
        }

        //When player lets go of it, and no longer holds it
        if (GetComponent<CharacterAiming>().IsHoldingProj() == false && holding == true)
        {
            audioData.Play(0);
            holding = false;
            Debug.Log("BALLING");
        }

    }

    public void Footsteps()
    {
        //empty for now
    }
}
