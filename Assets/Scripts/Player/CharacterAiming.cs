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
    [SerializeField] GameObject normalModel;
    [SerializeField] GameObject transparentModel;
    public onScreenTutorialText onScreenTutorial;
    [SerializeField] PauseMenu pauseMenu;

    [Header("Projectiles & Punching")]
    [SerializeField] [Range(1000.0f, 4000.0f)] float throwForce = 2000.0f;
    [SerializeField] [Range(10f, 200f)] float throwUpModifier = 200f;
    [SerializeField] [Range(100f, 1000f)] float chargePerSec = 500f;
    [SerializeField] [Range(1f, 5f)] float maxChargeTime = 3f;
    [SerializeField] [Range(0f, 20f)] float maxChangeFOV = 10f;
    [SerializeField] [Range(1.0f, 100.0f)] float punchForce = 20f;
    [SerializeField] [Range(0.5f, 10f)] float aimAssist = 2f;

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

    PowerUpScript powerup;

    GameObject aimAssistSphere;
    int playerNum;

    //BALLER

    //UserInput inputAction;

    private void OnEnable()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        playerNum = PlayerManager.instance.GetIndex(gameObject) + 1;
        aimAssistSphere = GameObject.Find("P" + playerNum + "AssistSphere");
        aimAssistSphere.SetActive(false);
        //inputAction = InputController.controller.inputAction;

        //inputAction.Player.Aim.performed += cntxt => OnAim();
        //inputAction.Player.Aim.canceled += cntxt => OnAim();

        //inputAction.Player.Fire.performed += cntxt => OnFire();

        //Lock the cursor, make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (gameObject == PlayerManager.instance.GetPlayer(1))
        {
            reticle = GameObject.Find("P1_UI").transform.Find("Reticle").GetComponent<Image>();
        }

        else if (gameObject == PlayerManager.instance.GetPlayer(2))
        {

            reticle = GameObject.Find("P2_UI").transform.Find("Reticle").GetComponent<Image>();
        }

        else if (gameObject == PlayerManager.instance.GetPlayer(3))
        {

            reticle = GameObject.Find("P3_UI").transform.Find("Reticle").GetComponent<Image>();
        }

        else if (gameObject == PlayerManager.instance.GetPlayer(4))
        {
            reticle = GameObject.Find("P4_UI").transform.Find("Reticle").GetComponent<Image>();
        }

        //if (gameObject.name == "P1") reticle = GameObject.Find("Reticle1").GetComponent<Image>();

        //else if (gameObject.name == "P2") reticle = GameObject.Find("Reticle2").GetComponent<Image>();

        else reticle = GameObject.Find("Reticle").GetComponent<Image>();

        //Hide the reticle
        reticle.enabled = false;
        animator = GetComponent<Animator>();

        //normalCamOrigFOV = normalCam.m_Lens.FieldOfView;
        zoomCamOrigFOV = zoomCam.m_Lens.FieldOfView;

        powerup = GetComponent<PowerUpScript>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(playerCam.pixelHeight / 2f + "      " + playerCam.pixelWidth / 2f);

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

        if (isAiming && holdingProjectile)
        {
            RaycastHit hitInfo;

            if (CheckForAimAssist(out hitInfo))
            {
                aimAssistSphere.SetActive(true);
                //Set the throw direction to be the vector pointing from the projectile's position to the raycast's point of contact
                aimAssistSphere.transform.position = hitInfo.point;

                Vector2 UIPoint = playerCam.WorldToViewportPoint(hitInfo.point);

                reticle.gameObject.GetComponent<RectTransform>().anchorMin = UIPoint;
                reticle.gameObject.GetComponent<RectTransform>().anchorMax = UIPoint;
                /*
                UIPoint.x *= reticle.gameObject.GetComponent<RectTransform>().sizeDelta.x;
                UIPoint.y *= reticle.gameObject.GetComponent<RectTransform>().sizeDelta.y;

                UIPoint.x -= reticle.gameObject.GetComponent<RectTransform>().sizeDelta.x * reticle.GetComponent<RectTransform>().pivot.x;
                UIPoint.y -= reticle.gameObject.GetComponent<RectTransform>().sizeDelta.y * reticle.GetComponent<RectTransform>().pivot.y;

                Debug.Log(UIPoint);
                
                reticle.gameObject.GetComponent<RectTransform>().anchoredPosition = UIPoint;
                */

                //reticle.gameObject.GetComponent<RectTransform>().position = new Vector3(hitInfo.point.x,
                //hitInfo.point.y, 0f);

                //Debug.Log(new Vector3(hitInfo.point.x,
                //    hitInfo.point.y, 0f));
            }

            else
            {
                reticle.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                reticle.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                aimAssistSphere.SetActive(false);
            }
            if (animator != null)
            {
                animator.SetBool("isAiming", isAiming);
                animator.SetBool("hasBall", holdingProjectile);
            }
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
        if (cntxt.performed && !pauseMenu.GetIsPaused())
        {
            zoomCam.Priority += 10;

            reticle.enabled = true;

            isAiming = true;

            if (holdingProjectile)
            {
                playerCam.cullingMask = playerCam.cullingMask & ~(1 << LayerMask.NameToLayer("Player" + (PlayerManager.instance.GetIndex(gameObject) + 1) + "Model"));
                playerCam.cullingMask = playerCam.cullingMask | (1 << LayerMask.NameToLayer("Player" + (PlayerManager.instance.GetIndex(gameObject) + 1) + "Transparent"));
            }
            if (holdingProjectile)
            {
                onScreenTutorial.showButton("Hold ", "Player/Fire", " to throw");
            }
            else
            {
                onScreenTutorial.show("");
            }
            
            Debug.Log("Has ball: " + holdingProjectile);
            //ParticleManager.instance.PlayEffect(transform.position, "RedParticles");
        }

        //If the player isn't aiming, prioritize the zoomed out camera and disable the reticle
        else if (cntxt.canceled && isAiming)
        {
            zoomCam.Priority -= 10;

            reticle.enabled = false;

            isAiming = false;
            if(holdingProjectile)
            {
                onScreenTutorial.showButton("Hold ", "Player/Aim", " to aim");
            }
            else
            {
                onScreenTutorial.show("");
            }
           
            playerCam.cullingMask = playerCam.cullingMask | (1 << LayerMask.NameToLayer("Player" + (PlayerManager.instance.GetIndex(gameObject) + 1) + "Model"));
            playerCam.cullingMask = playerCam.cullingMask & ~(1 << LayerMask.NameToLayer("Player" + (PlayerManager.instance.GetIndex(gameObject) + 1) + "Transparent"));

            //normalModel.SetActive(true);
            //transparentModel.SetActive(false);
           // Debug.Log("Has ball: " + holdingProjectile);
        }
	}

    public void OnFire(InputAction.CallbackContext cntxt)
    {
        if (cntxt.performed && holdingProjectile && isAiming)
            charging = true;
        onScreenTutorial.show("");
        //Debug.Log(charging);

        //If the player is holding a projectile and releases the throw button, then go through the steps to throw it
        if (holdingProjectile && cntxt.canceled && isAiming)
        {
            {
                Vector3 throwDir;

                reticle.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                reticle.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                aimAssistSphere.SetActive(false);

                playerCam.cullingMask = playerCam.cullingMask | (1 << LayerMask.NameToLayer("Player" + (PlayerManager.instance.GetIndex(gameObject) + 1) + "Model"));
                playerCam.cullingMask = playerCam.cullingMask & ~(1 << LayerMask.NameToLayer("Player" + (PlayerManager.instance.GetIndex(gameObject) + 1) + "Transparent"));

                //If we are aiming and the raycast hits something
                /*
                if (isAiming && Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hitInfo, 100f))
                {
                    //Set the throw direction to be the vector pointing from the projectile's position to the raycast's point of contact
                    throwDir = (hitInfo.point - heldProjectile.transform.position).normalized;
                }
                */

                /*
                if (isAiming && Physics.SphereCast(playerCam.transform.position, aimAssist, playerCam.transform.forward, out hitInfo, 100f, 
                    ~(LayerMask.NameToLayer("Player" + playerNum) | LayerMask.NameToLayer("Player" + playerNum + "Model") | LayerMask.NameToLayer("Player" + playerNum + "Transparent"))))
                {
                    //Set the throw direction to be the vector pointing from the projectile's position to the raycast's point of contact
                    throwDir = (hitInfo.point - heldProjectile.transform.position).normalized;
                }
                */
                RaycastHit hitInfo;

                if (CheckForAimAssist(out hitInfo))
                {
                    //Set the throw direction to be the vector pointing from the projectile's position to the raycast's point of contact
                    throwDir = (hitInfo.point - heldProjectile.transform.position).normalized;
                }

                else if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hitInfo, 100f))
                {
                    //Set the throw direction to be the vector pointing from the projectile's position to the raycast's point of contact
                    throwDir = (hitInfo.point - heldProjectile.transform.position).normalized;
                }

                //If the player isn't in aiming mode or if the raycast doesn't hit anything, just throw in the direction of the camera forward
                else
                    throwDir = playerCam.transform.forward;

                heldProjectile.GetComponent<TrailRenderer>().time = 0.3f;

                //heldProjectile.GetComponent<Collider>().enabled = true;

                heldProjectile.GetComponents<Collider>()[0].enabled = true;

                if (heldProjectile.GetComponents<Collider>().Length > 1)
                    heldProjectile.GetComponents<Collider>()[1].enabled = true;

                animator.SetTrigger("Throw");

                //Set the projectile back to non-kinematic
                heldProjectile.GetComponent<Rigidbody>().isKinematic = false;

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
                animator.SetBool("hasBall", holdingProjectile);
                Debug.Log("Has ball: " + holdingProjectile);

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

            if (animator != null && !animator.GetCurrentAnimatorStateInfo(1).IsName("Throw") && !animator.GetCurrentAnimatorStateInfo(1).IsName("Punch"))
            {                GetComponent<PlayerAudioController>().slapSFX();
                animator.SetTrigger("Punch");
            }

            GameObject playerObj = transform.GetChild(0).gameObject;

            RaycastHit hitInfo;
            //Physics.Raycast(transform.position, playerObj.transform.forward, out hitInfo, 100);

            //Physics.BoxCast(transform.position + new Vector3(0, 2, 0), new Vector3(2.5f, 1f, 0.1f), playerObj.transform.forward, out hitInfo, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), 5f);
            Physics.SphereCast(playerObj.transform.position - playerObj.transform.forward * 1.5f, 
                2f, playerObj.transform.forward, out hitInfo, 5f);

            if (hitInfo.collider != null && hitInfo.collider.gameObject.tag == "Player")
            {
                //GameObject hitPlayer = hitInfo.collider.transform.Find("PlayerObj").gameObject;
                hitInfo.collider.gameObject.transform.parent.GetComponent<Rigidbody>().AddForce(playerObj.transform.forward * punchForce + new Vector3 (0f, punchForce / 2, 0f), ForceMode.Impulse);
                Debug.Log("punch");                GetComponent<PlayerAudioController>().slaphitSFX();
                powerup.PlayerPunched();
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
        onScreenTutorial.showButton("Hold ", "Player/Aim", " to aim");

        //Tell the projectile that it isn't being held anymore
        if (heldProjectile.GetComponent<BallBehaviour>() != null)
        {
            heldProjectile.GetComponent<BallBehaviour>().SetIsHeld(true);
        }

        if (isAiming)
        {
            playerCam.cullingMask = playerCam.cullingMask & ~(1 << LayerMask.NameToLayer("Player" + (PlayerManager.instance.GetIndex(gameObject) + 1) + "Model"));
            playerCam.cullingMask = playerCam.cullingMask | (1 << LayerMask.NameToLayer("Player" + (PlayerManager.instance.GetIndex(gameObject) + 1) + "Transparent"));
           
        }

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

        //heldProjectile.GetComponent<Collider>().enabled = false;

        heldProjectile.GetComponents<Collider>()[0].enabled = false;
        
        if (heldProjectile.GetComponents<Collider>().Length > 1)
            heldProjectile.GetComponents<Collider>()[1].enabled = false;

        heldProjectile.GetComponent<TrailRenderer>().time = 0.1f;
    }

    public bool GetIsAiming()
    {
        return isAiming;
    }

    public float GetPunchForce()
    {
        return punchForce;
    }

    public void SetPunchForce(float newForce)
    {
        punchForce = newForce;
    }

    public float getThrowForce()
    {
        return throwForce;
    }

    public float getThrowModifier()
    {
        return throwUpModifier;
    }


    public GameObject getHeldProjectile()
    {
        return heldProjectile;
    }
    private bool CheckForAimAssist(out RaycastHit hitInfo)
    {
        if (playerNum == 1)
        {
            return Physics.SphereCast(playerCam.transform.position, aimAssist, playerCam.transform.forward, out hitInfo, 100f,
                (1 << LayerMask.NameToLayer("Player2") | 1 << LayerMask.NameToLayer("Player3") | 1 << LayerMask.NameToLayer("Player4")));
        }

        else if (playerNum == 2)
        {
            return Physics.SphereCast(playerCam.transform.position, aimAssist, playerCam.transform.forward, out hitInfo, 100f,
                (1 << LayerMask.NameToLayer("Player1") | 1 << LayerMask.NameToLayer("Player3") | 1 <<LayerMask.NameToLayer("Player4")));
        }

        else if (playerNum == 3)
        {
            return Physics.SphereCast(playerCam.transform.position, aimAssist, playerCam.transform.forward, out hitInfo, 100f,
                (1 << LayerMask.NameToLayer("Player1") | 1 << LayerMask.NameToLayer("Player2") | 1 << LayerMask.NameToLayer("Player4")));
        }

        else if (playerNum == 4)
        {
            return Physics.SphereCast(playerCam.transform.position, aimAssist, playerCam.transform.forward, out hitInfo, 100f,
                (1 << LayerMask.NameToLayer("Player1") | 1 << LayerMask.NameToLayer("Player2") | 1 << LayerMask.NameToLayer("Player3")));
        }

        else
        {
            Physics.SphereCast(playerCam.transform.position, aimAssist, playerCam.transform.forward, out hitInfo, 100f);
            Debug.Log("Spherecasting problem!!!!");
            return false;
        }

    }
}
