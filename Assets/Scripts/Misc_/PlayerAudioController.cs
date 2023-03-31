using GameAudioScriptingEssentials;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    public float timeBetweenFootsteps = 0.5f;
    float soundCooldown;
    bool holding = false;
    public AudioSource audioSource;
    public AudioClip audio1; //Throw
    public AudioClip audio2; //Jump
    public AudioClip audio3; //Jump Land
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1;
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
            audioSource.clip = audio1;
            audioSource.volume = 1f;
            audioSource.Play();
            holding = false;
           // Debug.Log("BALLING");
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.layer == 6 || collision.gameObject.layer == 6) && GetComponent<TpMovement>().GetGroundPos() < GetComponent<TpMovement>().GetJumpPos())
        {
            audioSource.clip = audio3;
            audioSource.volume = 0.75f;
            audioSource.Play();
            GetComponent<TpMovement>().SetGroundPos(0f);
            GetComponent<TpMovement>().SetJumpPos(0f);
        }
    }

    public void jumpSFX()
    {
        audioSource.clip = audio2;
        audioSource.volume = 1f;
        audioSource.Play();
    }
}
