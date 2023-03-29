using GameAudioScriptingEssentials;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    public float timeBetweenFootsteps = 0.5f;
    float soundCooldown;
    bool holding = false;
    public AudioSource audio1;
    public AudioSource audio2;
    public AudioSource audio3;
    void Start()
    {

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
            audio1.Play();
            holding = false;
           // Debug.Log("BALLING");
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.layer == 6 || collision.gameObject.layer == 6) && GetComponent<TpMovement>().GetGroundPos() < GetComponent<TpMovement>().GetJumpPos())
        {
            audio3.Play();
            GetComponent<TpMovement>().SetGroundPos(0f);
            GetComponent<TpMovement>().SetJumpPos(0f);
        }
    }

    public void Footsteps()
    {
        //empty for now
    }

    public void jumpSFX()
    {
        audio2.Play();
    }

    public void landingSFX()
    {
        audio3.Play();
    }
}
