using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugControls : MonoBehaviour
{
    [Header("Spawnables")]
    [SerializeField] private GameObject playerDummy;

    bool editing = false;
    bool paused = false;

    PlayerInput pInput;

    // Start is called before the first frame update
    void Start()
    {
        pInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnToggleEditor()
    {
        //Enable/disable editing bool
        editing = !editing;

        //Enable/disable the editor action map, allowwing/disallowing editor actions
        if (editing) pInput.actions.FindActionMap("Editor").Enable();

        else pInput.actions.FindActionMap("Editor").Disable();
    }


    //-------------------The following actions are only perfomrable while the Editor action map is enabled--------------------------\\

    public void OnEditorPause()
    {
        paused = !paused;

        //Pause the game if pausing
        if (paused)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        //Unpause the game if unpausing
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnSpawnDummy(InputAction.CallbackContext cntxt)
    {
        //If the button was just pressed
        if (cntxt.performed)
        {
            //Find specifically the PlayerObj object within the current player (to use its forward)
            GameObject playerObj = transform.Find("PlayerObj").gameObject;

            //Instantiate a dummy player
            GameObject newDummy = Instantiate(playerDummy);

            //Freeze its rotation
            newDummy.GetComponent<Rigidbody>().freezeRotation = true;

            //Set its position in front of the current player
            newDummy.transform.position = transform.position + playerObj.transform.forward * 10f;
        }
    }
}
