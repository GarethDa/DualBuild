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
    [SerializeField] CinemachineVirtualCamera normalCam;
    [SerializeField] CinemachineVirtualCamera zoomCam;

    [Header("Projectiles & Punching")]
    [SerializeField] [Range(1000.0f, 4000.0f)] float throwForce = 2000.0f;
    [SerializeField] [Range(10f, 200f)] float throwUpModifier = 200f;
    [SerializeField] [Range(100f, 1000f)] float chargePerSec = 500f;
    [SerializeField] [Range(1f, 5f)] float maxChargeTime = 3f;
    [SerializeField] [Range(0f, 20f)] float maxChangeFOV = 10f;
    [SerializeField] [Range(1.0f, 100.0f)] float punchForce = 20f;

    Image reticle;

    bool isAiming = false;
    bool holdingProjectile = false;

    GameObject heldProjectile = null;
    Animator animator;

    //Parameters for charging projectile throwing
    bool charging = false;
    float chargeTime = 0f;
    float chargedForce = 0f;
    //float normalCamOrigFOV;
    float zoomCamOrigFOV;
    float returnTimeFOV = 0f;

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

        //normalCamOrigFOV = normalCam.m_Lens.FieldOfView;
        zoomCamOrigFOV = zoomCam.m_Lens.FieldOfView;

    }

    // Update is called once per frame
    void Update()
    {
        if (animator != null)
        {
            animator.SetBool("isAiming", isAiming);
            animator.SetBool("hasBall", holdingProjectile);
        }
        
        if (isAiming)
        {
            //If the player is aiming, set the player object's rotation around the y-axis to that of the camera
            playerObj.transform.rotation = Quaternion.Euler(new Vector3(0f, playerCam.transform.rotation.eulerAngles.y, 0f));
            //transform.Find("Orientation").rotation = Quaternion.Euler(new Vector3(0f, playerCam.transform.rotation.eulerAngles.y, 0f));
        }

        ChargeProjectile();
    }

    private void FixedUpdate()
    {
    }

    //Function for charging projectile throwing speed and altering the camera FOV accordingly
    public void ChargeProjectile()
    {
        //If we are charging
        if (charging)
        {
            //Increase the charge time
            chargeTime += Time.deltaTime;

            //If the charge time hasn't reached the max
            if (chargeTime <= maxChargeTime)
            {
                //Add to the extra throw force
                chargedForce += chargePerSec * Time.deltaTime;

                //Zoom in
                //normalCam.m_Lens.FieldOfView = normalCamOrigFOV - (chargeTime / maxChargeTime) * maxChangeFOV;
                zoomCam.m_Lens.FieldOfView = zoomCamOrigFOV - (chargeTime / maxChargeTime) * maxChangeFOV;
            }

            //If the charge time has reached the max
            else
            {
                //Max out the extra throw force
                chargedForce = chargePerSec * maxChargeTime;
                //Debug.Log("Max force! " + chargedForce);

                //Fully zoom the cameras
                //normalCam.m_Lens.FieldOfView = normalCamOrigFOV - maxChangeFOV;
                zoomCam.m_Lens.FieldOfView = zoomCamOrigFOV - maxChangeFOV;
            }
        }

        //If the return time for FOV is greater than zero, meaning we should be returning the FOV to normal
        else if (returnTimeFOV > 0f)
        {
            //Reduce the return time left
            returnTimeFOV -= Time.deltaTime;

            //If the return time is less than zero, set it back to zero
            if (returnTimeFOV < 0f)
                returnTimeFOV = 0f;

            //Set the camera zoom accordingly
            //normalCam.m_Lens.FieldOfView = normalCamOrigFOV - (returnTimeFOV / 0.3f) * maxChangeFOV;
            zoomCam.m_Lens.FieldOfView = zoomCamOrigFOV - (returnTimeFOV / 0.3f) * maxChangeFOV;
        }
    }

	public void OnAim(InputAction.CallbackContext cntxt)
	{
        //If the player is aiming, prioritize the zoomed in camera and enable to reticle
        if (cntxt.performed)
        {
            zoomCam.Priority += 10;

            reticle.enabled = true;

            isAiming = true;

            
            //ParticleManager.instance.PlayEffect(transform.position, "RedParticles");
        }

        //If the player isn't aiming, prioritize the zoomed out camera and disable the reticle
        else if (cntxt.canceled)
        {
            zoomCam.Priority -= 10;

            reticle.enabled = false;

            isAiming = false;
        }
	}

    public void OnFire(InputAction.CallbackContext cntxt)
    {
        if (cntxt.performed && holdingProjectile && isAiming)
            charging = true;

        //Debug.Log(charging);

        //If the player is holding a projectile and releases the throw button, then go through the steps to throw it
        if (holdingProjectile && cntxt.canceled && isAiming)
        {
            {
                heldProjectile.GetComponent<TrailRenderer>().time = 0.3f;

                heldProjectile.GetComponent<Collider>().enabled = true;

                animator.SetTrigger("Throw");
                //Set the projectile back to non-kinematic
                heldProjectile.GetComponent<Rigidbody>().isKinematic = false;

                Vector3 throwDir;

                RaycastHit hitInfo;

                //If we are aiming and the raycast hits something
                if (isAiming && Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hitInfo, 100f))
                {
                    //Set the throw direction to be the vector pointing from the projectile's position to the raycast's point of contact
                    throwDir = (hitInfo.point - heldProjectile.transform.position).normalized;
                }

                //If the player isn't in aiming mode or if the raycast doesn't hit anything, just throw in the direction of the camera forward
                else
                    throwDir = playerCam.transform.forward;

                //If the player isn't aiming, move the ball forward a bit.
                //This is to stop the ball from hitting the player the moment it is thrown
                if (!isAiming)
                    heldProjectile.transform.position += new Vector3(2 * playerCam.transform.forward.x, 0f, 2 * playerCam.transform.forward.z);

                //Throw the ball forward, multiplied by the throwing force
                heldProjectile.GetComponent<Rigidbody>().AddForce(throwDir * (throwForce + chargedForce) + Vector3.up * (throwUpModifier));

                //Return all the charging parameters back to 0
                chargedForce = 0f;
                chargeTime = 0f;
                charging = false;

                //Start returning the camera FOVs to normal
                returnTimeFOV = 0.3f * -((zoomCam.m_Lens.FieldOfView - zoomCamOrigFOV) / maxChangeFOV);

                //The player is no longer holding a projectile
                holdingProjectile = false;

                //The ball shouldn't be a child of the player model anymore
                heldProjectile.transform.SetParent(null);

                //Tell the projectile that it isn't being held anymore
                if (heldProjectile.GetComponent<BallBehaviour>() != null)
                {
                    heldProjectile.GetComponent<BallBehaviour>().SetIsHeld(false);
                }

                //Tell the projectile that it has been thrown 
                if (heldProjectile.GetComponent<BallBehaviour>() != null)
                {
                    heldProjectile.GetComponent<BallBehaviour>().SetIsThrown(true);
                }
                else if (heldProjectile.GetComponent<BombBehaviour>() != null)
                {
                    heldProjectile.GetComponent<BombBehaviour>().setThrown(true);
                    Debug.Log("thrown");
                }
            }
        }

        else if (!isAiming && cntxt.performed)
        {

            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Throw"))
                animator.SetTrigger("Throw");

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

        heldProjectile.GetComponent<TrailRenderer>().time = 0.1f;
    }

    public bool GetIsAiming()
    {
        return isAiming;
    }
}
