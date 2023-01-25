using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cinemachine;

public class CharacterAiming : MonoBehaviour
{
    [Header("Required Objects")]
    [SerializeField] Camera playerCam;
    [SerializeField] Transform playerObj;
    [SerializeField] Transform holdPos;
    [SerializeField] CinemachineVirtualCamera zoomCam;

    [Header("Projectiles & Punching")]
    [SerializeField] [Range(1000.0f, 4000.0f)] float throwForce = 2000.0f;
    [SerializeField] [Range(1.0f, 100.0f)] float punchForce = 20f;

    Image reticle;

    bool isAiming = false;
    bool holdingProjectile = false;

    GameObject heldProjectile = null;
    Animator animator;
    //BALLER

    //UserInput inputAction;

    private void OnEnable()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        //inputAction = InputController.controller.inputAction;

        //inputAction.Player.Aim.performed += cntxt => OnAim();
        //inputAction.Player.Aim.canceled += cntxt => OnAim();

        //inputAction.Player.Fire.performed += cntxt => OnFire();

        //Lock the cursor, make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (gameObject.name == "P1") reticle = GameObject.Find("Reticle1").GetComponent<Image>();

        else if (gameObject.name == "P2") reticle = GameObject.Find("Reticle2").GetComponent<Image>();

        else reticle = GameObject.Find("Reticle").GetComponent<Image>();

        //Hide the reticle
        reticle.enabled = false;
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        if (animator != null)
        {
            animator.SetBool("isAiming", isAiming);
            animator.SetBool("hasBall", holdingProjectile);
        }
        else
        {
            Debug.Log("Drink your grandson's pee");
        }
        if (isAiming)
        {
            //If the player is aiming, set the player object's rotation around the y-axis to that of the camera
            playerObj.transform.rotation = Quaternion.Euler(new Vector3(0f, playerCam.transform.rotation.eulerAngles.y, 0f));
            //transform.Find("Orientation").rotation = Quaternion.Euler(new Vector3(0f, playerCam.transform.rotation.eulerAngles.y, 0f));
        }
        
    }

    private void FixedUpdate()
    {
    }

	//Aiming is a value type, not a button type, meaning it can sense when aim is being held down and released
	public void OnAim(InputAction.CallbackContext cntxt)
	{
        //If the player is aiming, prioritize the zoomed in camera and enable to reticle
        if (cntxt.performed)
        {
            zoomCam.Priority += 10;

            reticle.enabled = true;

            isAiming = true;

            
            ParticleManager.instance.PlayEffect(transform.position, "RedParticles");
        }

        //If the player isn't aiming, prioritize the zoomed out camera and disable the reticle
        else if (cntxt.canceled)
        {
            zoomCam.Priority -= 10;

            reticle.enabled = false;

            isAiming = false;
        }
	}

    public void OnFire()
    {
        //If the player is holding a projectile, then go through the steps to throw it
        if (holdingProjectile)
        {
            heldProjectile.GetComponent<Collider>().enabled = true;

            animator.SetTrigger("Throw");
            //Set the projectile back to non-kinematic
            heldProjectile.GetComponent<Rigidbody>().isKinematic = false;

            //Throw the ball forward, multiplied by the throwing force
            heldProjectile.GetComponent<Rigidbody>().AddForce(playerCam.transform.forward * throwForce + Vector3.up * (throwForce / 10));

            //The player is no longer holding a projectile
            holdingProjectile = false;

            //The ball shouldn't be a child of the player model anymore
            heldProjectile.transform.SetParent(null);

            //Tell the projectile that it isn't being held anymore
            heldProjectile.GetComponent<BallBehaviour>().SetIsHeld(false);

            //Tell the projectile that it has been thrown 
            heldProjectile.GetComponent<BallBehaviour>().SetIsThrown(true);
        }

        else
        {
            GameObject playerObj = transform.Find("PlayerObj").gameObject;

            RaycastHit hitInfo;
            //Physics.Raycast(transform.position, playerObj.transform.forward, out hitInfo, 100);

            Physics.BoxCast(transform.position, new Vector3(1f, 1f, .1f), playerObj.transform.forward, out hitInfo, Quaternion.identity, 4f);

            if (hitInfo.collider != null && hitInfo.collider.gameObject.tag == "Player")
            {
                //GameObject hitPlayer = hitInfo.collider.transform.Find("PlayerObj").gameObject;
                hitInfo.collider.gameObject.transform.parent.GetComponent<Rigidbody>().AddForce(playerObj.transform.forward * punchForce + new Vector3 (0f, punchForce, 0f), ForceMode.Impulse);
                Debug.Log("punch");
            }
        }
    }

    //Function for telling whether the player is holding a projectile or not
    public bool IsHoldingProj()
    {
        return holdingProjectile;
    }

    //Function for locking a projectile to the player
    public void SetProjectile(GameObject projectile)
    {
        //Set holding projectile to true, and save the projectile being held
        holdingProjectile = true;
        heldProjectile = projectile;

        //Set the projectile's position to be in front of the player model, with some offset
        /*
        heldProjectile.transform.position = playerObj.transform.position 
            + playerObj.transform.forward.normalized 
            + (0.8f * playerObj.transform.right.normalized) 
            + (3f * playerObj.transform.up.normalized);
        */
        heldProjectile.transform.position = holdPos.transform.position;

        //Parent the player model to the projectile
        heldProjectile.transform.SetParent(holdPos.transform);

        //Set the projectile to kinematic, ensuring it doesn't move while being held
        heldProjectile.GetComponent<Rigidbody>().isKinematic = true;

        heldProjectile.GetComponent<Collider>().enabled = false;
    }

    public bool GetIsAiming()
    {
        return isAiming;
    }
}
