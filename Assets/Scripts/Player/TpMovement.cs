using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;

public class TpMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] [Range(3.0f, 40.0f)] private float jumpForce = 20.0f;
    [SerializeField] [Range(5.0f, 30.0f)] private float rotSpeed = 10.0f;
	[SerializeField] [Range(5.0f, 50.0f)] private float moveSpeed = 20.0f;
    //private float dragVariable = 1.0f;
    [SerializeField] [Range(1.0f, 100.0f)] private float jumpGravity = 9.8f;
    [SerializeField] [Range(1.0f, 4.0f)] private float fallMultiplier = 1.0f;
    [SerializeField] [Range(0f, 10.0f)] private float groundDrag = 1.0f;
    [SerializeField] PhysicMaterial physMatFriction;
    [SerializeField] PhysicMaterial physMatFrictionless;
    [SerializeField] [Range(1.0f, 5.0f)] float airControlDivisor = 2.0f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask floorMask;
    [SerializeField] private Transform feetTransform;
    [SerializeField] [Range(0f, 1f)] private float coyoteTime = 0.2f;

    [Header("Rotation")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObj;

    [Header("Camera")]
	[SerializeField] private Camera playerCam;
    [SerializeField] private PauseMenu settingsMenu;

    Vector2 moveInput;
    float horizontalInput;
    float verticalInput;

    float jumpPos;
    float groundPos;

    Vector3 moveDir;

    Rigidbody rBody;

    bool upHeld = false;
    bool downHeld = false;
    bool leftHeld = false;
    bool rightHeld = false;

    bool isGrounded;
    bool lastFrameGrounded = true;
    bool justSwapped = false;

    float coyoteTimer = 0f;
    float jumpTimer = 0f;

    RaycastHit rayHit;

    bool editing = false;

    private bool isRunning;

    private Animator animator;

    private PowerUpScript powerup;

    float initialSpeed;
    bool enableHitTimer = false;
    float hitTimer = 0;

    public onScreenTutorialText screenTutorial;
    //UserInput inputAction;
    

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        //screenTutorial = GetComponent<onScreenTutorialText>();
        //Freeze the rotation of the rigid body, ensuring it doesn't fall over
        rBody.freezeRotation = true;
        RoundManager.instance.totalPlayers++;
        animator = GetComponent<Animator>();

        //GameObject.Find("EditorCanvas").GetComponent<Canvas>().enabled = false;

        rBody.drag = 0f;

        powerup = GetComponent<PowerUpScript>();

        //settingsMenu = gameObject.transform.Find("P1_UI").GetComponent<PauseMenu>();

        jumpTimer = coyoteTime;

        initialSpeed = GetSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        if (enableHitTimer)
        {
            hitTimer += Time.deltaTime;

            if (hitTimer >= 2)
            {
                SetSpeed(initialSpeed);
                Debug.Log("Reset speed to " + initialSpeed);
                enableHitTimer = false;
                hitTimer = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(feetTransform.position, 0.1f, floorMask);
        animator.SetBool("isGrounded", coyoteTimer > 0f);

        jumpTimer += Time.deltaTime;

        if (isGrounded && jumpTimer >= coyoteTime)
        {
            rBody.drag = groundDrag;

            coyoteTimer = coyoteTime;
            //physMat.dynamicFriction = 3.5f;
        }

        else
        {
            rBody.drag = 0f;

            coyoteTimer -= Time.deltaTime;

            //physMat.dynamicFriction = 0f;
        }

        if (isRunning /*|| (rBody.velocity.x == 0f && rBody.velocity.z == 0f)*/)
            playerObj.GetComponent<Collider>().material = physMatFrictionless;
            //physMat.dynamicFriction = 0f;

        else
            playerObj.GetComponent<Collider>().material = physMatFriction;
            //physMat.dynamicFriction = 3.5f;

        if (isGrounded && !lastFrameGrounded) justSwapped = true;

        RotatePlayer();
        MovePlayer();
        LimitSpeed();
        //AddHorizontalDrag();

        lastFrameGrounded = isGrounded;
        justSwapped = false;
        
        //Debug.Log(Mathf.Sqrt(Mathf.Pow(rBody.velocity.x, 2) + Mathf.Pow(rBody.velocity.z, 2)));
        
        if (rBody.velocity.y < 0)
        {
            rBody.velocity += Vector3.up * Physics.gravity.y * fallMultiplier * Time.deltaTime;
        }

       // Debug.Log(new Vector3(rBody.velocity.x, 0f, rBody.velocity.z).magnitude);

    }

    //For rotating the player object when the player inputs a direction
	private void RotatePlayer()
	{
        
        

        //Rotate orientation

        Vector3 viewDir;
        if (GetComponent<CharacterAiming>().GetIsAiming())
            viewDir = new Vector3(playerCam.transform.forward.x, 0f, playerCam.transform.forward.z).normalized;

        else
            viewDir = transform.position - new Vector3(playerCam.transform.position.x, transform.position.y, playerCam.transform.position.z);
		
        orientation.forward = viewDir.normalized;

        //Determine the input direction based on the current orientation of the player model
		Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

		//If the player has input a movement, spherically lerp between the current forward and the new direction
		if (inputDir != Vector3.zero) playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotSpeed);
	}

    //For moving the player object when the player inputs a direction
    private void MovePlayer()
    {
        //rBody.useGravity = true;
        
        //Calculate direction
        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        bool onRamp = false;

        if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f))
        {
            //Create a quaternion that holds the rotation from up to along the ramp
            float groundRot = Vector3.Angle(Vector3.up, rayHit.normal);

            //Create a new velocity by multiplying the rotation quaternion with the current velocity
            //Vector3 newVelocity = groundRot * rBody.velocity;

            //If the y component of the velocity is less than 0, meaning the player is going down a ramp,
            //then set the velocity to the new velocity
            //if (newVelocity.y < 0) rBody.velocity = newVelocity;

            if (groundRot > 0)
            {
                onRamp = true;
                moveDir = Vector3.ProjectOnPlane(moveDir, rayHit.normal).normalized;
                rBody.useGravity = false;
                //Debug.Log("Ramp Velocity: " + rBody.velocity.magnitude);

                if (rBody.velocity.y > 0)
                {
                    rBody.AddForce(Vector3.down * 80f, ForceMode.Force);
                }
            }

            else
            {
                //Debug.Log("Original velocity: " + rBody.velocity.magnitude);
                rBody.useGravity = true;
            }
        }

        else
        {
            rBody.useGravity = true;
        }


        if (coyoteTimer <= 0)
        {
            rBody.AddForce(moveDir.normalized * moveSpeed * 5f / airControlDivisor, ForceMode.Force);

            rBody.AddForce(new Vector3(0f, -jumpGravity, 0f), ForceMode.Force);
        }

        else
            rBody.AddForce(moveDir.normalized * moveSpeed * 5f, ForceMode.Force);

        if (justSwapped && !onRamp)
        {
            rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        }

        /*
        //Debug.Log("Original velocity: " + rBody.velocity.magnitude);

        //This code is for keeping the player on a ramp while they are doing down it.
        //Raycast downwards to see what the player is standing on.
        if (Physics.Raycast(transform.position, -transform.up, out rayHit, 0.5f))
        {
            //Create a quaternion that holds the rotation from up to along the ramp
            Quaternion groundRot = Quaternion.FromToRotation(Vector3.up, rayHit.normal);

            //Create a new velocity by multiplying the rotation quaternion with the current velocity
            Vector3 newVelocity = groundRot * rBody.velocity;

            //If the y component of the velocity is less than 0, meaning the player is going down a ramp,
            //then set the velocity to the new velocity
            if (newVelocity.y < 0) rBody.velocity = newVelocity;

            Debug.Log("Ramp Velocity: " + rBody.velocity.magnitude);
        }
        */
    }

    private void LimitSpeed()
    {
        Vector3 currentSpd = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);

        if (currentSpd.magnitude > moveSpeed)
        {
            Vector3 newSpd = currentSpd.normalized * moveSpeed;
            rBody.velocity = new Vector3(newSpd.x, rBody.velocity.y, newSpd.z);
        }
    }

    //For removing slipperiness (OLD, DON'T USE)
	private void AddHorizontalDrag()
	{
        Vector3 horizontalVel = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        Vector3 dragForce = -groundDrag * horizontalVel * horizontalVel.magnitude;

        rBody.AddForce(dragForce, ForceMode.Force);
    }
    public void OnAim(InputAction.CallbackContext cntxt)
    {
        if (cntxt.action.name.Equals("Look"))
        {
            if (!screenTutorial.hasShownTutorialType[(int)currentTutorialType.LOOK])
            {
                if(screenTutorial.getAnalogueName(screenTutorial.getButtonString("Player/Up")).Contains("stick"))
                    {
                    screenTutorial.show("Use " + screenTutorial.getAnalogueName(screenTutorial.getButtonString("Player/Up"))+
                     " to move around");
                }
                else
                {
                    screenTutorial.show("Use " + screenTutorial.getActualButtonName(screenTutorial.getButtonString("Player/Up")) + "/"
                    + screenTutorial.getActualButtonName(screenTutorial.getButtonString("Player/Left")) + "/"
                    + screenTutorial.getActualButtonName(screenTutorial.getButtonString("Player/Down")) + "/"
                    + screenTutorial.getActualButtonName(screenTutorial.getButtonString("Player/Right")) + " to move around");
                }
                
                screenTutorial.hideTutorial(currentTutorialType.LOOK);
            }
        }
    }

    
        //New input system
        public void OnMove(InputAction.CallbackContext cntxt)
	{
        if (settingsMenu.GetIsPaused())
        {
            return;
        }
        if (!screenTutorial.hasShownTutorialType[(int)currentTutorialType.MOVE])
        {
            screenTutorial.show("Use " + screenTutorial.getActualButtonName(screenTutorial.getButtonString("Player/Jump")) + " to jump");
            screenTutorial.hideTutorial(currentTutorialType.MOVE);
        }
        //Use threshold checks for shitty gamepads
        if (cntxt.action.name.Equals("Up"))
        {
            if (cntxt.performed && cntxt.ReadValue<float>() >= 0.4f) upHeld = true;

            if (cntxt.canceled || cntxt.ReadValue<float>() < 0.4f) upHeld = false;
        }

        if (cntxt.action.name.Equals("Down"))
        {
            if (cntxt.performed && cntxt.ReadValue<float>() >= 0.4f) downHeld = true;

            if (cntxt.canceled || cntxt.ReadValue<float>() < 0.4f) downHeld = false;
        }

        if (cntxt.action.name.Equals("Left"))
        {
            if (cntxt.performed && cntxt.ReadValue<float>() >= 0.4f) leftHeld = true;

            if (cntxt.canceled || cntxt.ReadValue<float>() < 0.4f) leftHeld = false;
        }

        if (cntxt.action.name.Equals("Right"))
        {
            if (cntxt.performed && cntxt.ReadValue<float>() >= 0.4f) rightHeld = true;

            if (cntxt.canceled || cntxt.ReadValue<float>() < 0.4f) rightHeld = false;
        }

        verticalInput = Convert.ToInt32(upHeld) - Convert.ToInt32(downHeld);
        horizontalInput = Convert.ToInt32(rightHeld) - Convert.ToInt32(leftHeld);

        //horizontalInput = playerMovement.x;
        //verticalInput = playerMovement.y;
        //animation stuff

        isRunning = ((horizontalInput != 0) || (verticalInput != 0)) ? true : false;
        animator.SetBool("isRunning", isRunning);
    }

    //New input system
    public void OnJump(InputAction.CallbackContext cntxt)
    {
        if (settingsMenu.GetIsPaused())
        {
            return;
        }

        if (!screenTutorial.hasShownTutorialType[(int)currentTutorialType.JUMP])
        {
            screenTutorial.hideTutorial(currentTutorialType.JUMP);
        }

        if (cntxt.performed)
        {
            if (coyoteTimer > 0f)
            {
                groundPos = gameObject.transform.position.y;
                GetComponent<PlayerAudioController>().jumpSFX();
                rBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                //ParticleManager.instance.PlayEffect(transform.position, "WhiteParticles");
                powerup.PlayerJumped();
                StartCoroutine(CheckJump());

                coyoteTimer = 0f;
                jumpTimer = 0f;
            }
        }
    }

    //Check if the player jumped
    IEnumerator CheckJump()
    {
        yield return new WaitForSeconds(0.8f);
        jumpPos = gameObject.transform.position.y;
    }

    public void OnPause()
    {
        settingsMenu.OnPause();
    }

    //For setting the jump force
    public void SetJumpForce(float newForce)
    {
        jumpForce = newForce;
    }

    //For getting the jump force
    public float GetJumpForce()
    {
        return jumpForce;
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    //For getting the jump force
    public float GetSpeed()
    {
        return moveSpeed;
    }

    public void SetJumpPos(float newPos)
    {
        jumpPos = newPos;
    }

    public float GetJumpPos()
    {
        return jumpPos;
    }

    public void SetGroundPos(float newPos)
    {
        groundPos = newPos;
    }

    public float GetGroundPos()
    {
        return groundPos;
    }
    
    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    public void HitStun()
    {
        enableHitTimer = true;
        GetComponent<ParticleSystem>().Play();
        SetSpeed(10);
        Debug.Log("Speed Set to 10");
        hitTimer = 0;
        Debug.Log("Hit Timer reset to 0");
    }
}
