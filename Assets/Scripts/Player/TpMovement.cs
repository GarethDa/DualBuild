using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
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

    [Header("Ground Check")]
    [SerializeField] private LayerMask floorMask;
    [SerializeField] private Transform feetTransform;

    [Header("Rotation")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObj;

    [Header("Camera")]
	[SerializeField] private Camera playerCam;

    Vector2 moveInput;
    float horizontalInput;
    float verticalInput;

    Vector3 moveDir;

    Rigidbody rBody;

    PhotonView view;

    bool upHeld = false;
    bool downHeld = false;
    bool leftHeld = false;
    bool rightHeld = false;

    bool isGrounded;
    bool lastFrameGrounded = true;
    bool justSwapped = false;

    RaycastHit rayHit;

    bool editing = false;

    private bool isRunning;

    private Animator animator;

    //UserInput inputAction;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
       
        //Freeze the rotation of the rigid body, ensuring it doesn't fall over
        rBody.freezeRotation = true;

        //Photon component attached to player
        view = GetComponent<PhotonView>();

        animator = GetComponent<Animator>();

        //GameObject.Find("EditorCanvas").GetComponent<Canvas>().enabled = false;

        rBody.drag = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        //if you control THAT character
        /*        if (view.IsMine)        {
        this is where photon stuff goes if put back in
        }
        
        else
        {
            GetComponent<TpMovement>().enabled = false;
        }
        */

        isGrounded = Physics.CheckSphere(feetTransform.position, 0.1f, floorMask);
        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded) rBody.drag = groundDrag;

        else
        {
            rBody.drag = 0f;
        }

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

        //Debug.Log(new Vector3(rBody.velocity.x, 0f, rBody.velocity.z).magnitude);

    }

    //For rotating the player object when the player inputs a direction
	private void RotatePlayer()
	{
		//Rotate orientation
		Vector3 viewDir = transform.position - new Vector3(playerCam.transform.position.x, transform.position.y, playerCam.transform.position.z);
		orientation.forward = viewDir.normalized;

        //Determine the input direction based on the current orientation of the player model
		Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

		//If the player has input a movement, spherically lerp between the current forward and the new direction
		if (inputDir != Vector3.zero) playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotSpeed);
	}

    //For moving the player object when the player inputs a direction
    private void MovePlayer()
    {
       
        //Calculate direction
        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rBody.AddForce(moveDir.normalized * moveSpeed * 5f, ForceMode.Force);

        if (!isGrounded)
        {
            rBody.AddForce(new Vector3(0f, -jumpGravity, 0f), ForceMode.Force);
        }

        if (justSwapped)
        {
            rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        }

        //This code is for keeping the player on a ramp while they are doing down it.
        //Raycast downwards to see what the player is standing on.
        if (Physics.Raycast(feetTransform.position, -transform.up, out rayHit, 0.1f))
        {
            //Create a quaternion that holds the rotation from up to along the ramp
            Quaternion groundRot = Quaternion.FromToRotation(Vector3.up, rayHit.normal);

            //Create a new velocity by multiplying the rotation quaternion with the current velocity
            Vector3 newVelocity = groundRot * rBody.velocity;

            //If the y component of the velocity is less than 0, meaning the player is going down a ramp,
            //then set the velocity to the new velocity
            if (newVelocity.y < 0) rBody.velocity = newVelocity;
        }
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

    //New input system
    public void OnMove(InputAction.CallbackContext cntxt)
	{
        //Vector2 playerMovement = cntxt.ReadValue<Vector2>();

        if (cntxt.action.name.Equals("Up"))
        {
            if (cntxt.performed) upHeld = true;

            if (cntxt.canceled) upHeld = false;
        }

        if (cntxt.action.name.Equals("Down"))
        {
            if (cntxt.performed) downHeld = true;

            if (cntxt.canceled) downHeld = false;
        }

        if (cntxt.action.name.Equals("Left"))
        {
            if (cntxt.performed) leftHeld = true;

            if (cntxt.canceled) leftHeld = false;
        }

        if (cntxt.action.name.Equals("Right"))
        {
            if (cntxt.performed) rightHeld = true;

            if (cntxt.canceled) rightHeld = false;
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
        if (cntxt.performed)
        {
            if (isGrounded)
            {
                rBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                ParticleManager.instance.PlayEffect(transform.position, "WhiteParticles");
            }
        }
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

    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    /*
    void OnEnableUI()
    {
        GameObject.Find("EditorCanvas").GetComponent<Canvas>().enabled = !GameObject.Find("EditorCanvas").GetComponent<Canvas>().enabled;

        editing = !editing;

        playerCam.GetComponent<CinemachineBrain>().enabled = !playerCam.GetComponent<CinemachineBrain>().enabled;

        Cursor.visible = !Cursor.visible;

        if (editing) Cursor.lockState = CursorLockMode.None;

        else Cursor.lockState = CursorLockMode.Locked;

        //spawnerUI.enabled = !spawnerUI.enabled;
    }
    */

    public void OnEditorPause()
    {
        editing = !editing;

        if (editing)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
