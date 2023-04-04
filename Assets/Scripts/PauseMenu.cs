using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    //A struct which holds a reference to an input action and the button which will rebind it
    [Serializable]
    private struct InputInfoStruct
    {
        public InputAction actionReference;
        public GameObject actionButton;
    }

    [Header("Scene/Screen switching")]
    [SerializeField] private GameObject pauseScreen;

    [Header("Control rebinding")]
    [SerializeField] private List<InputInfoStruct> inputInfoList = new List<InputInfoStruct>();

    [Header("Sliders")]
    [SerializeField] private Slider zoomedInSensitivity;
    [SerializeField] private Slider zoomedOutSensitivity;

    [SerializeField] GameObject settingsFirst;
    [SerializeField] GameObject chatBox;

    //[SerializeField] private PlayerInput pInput;
    //A list of texts which display the current binding of each action
    private List<TMP_Text> bindingTexts = new List<TMP_Text>();

    //A list of the same text's game objects
    private List<GameObject> rebindTextObjects = new List<GameObject>();

    //A list of the text which appears to tell the player that the system is waiting for an input
    private List<GameObject> waitingTextObjects = new List<GameObject>();

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    bool paused = false;

    int playerNum;

    float rebindTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("GameManager").GetComponent<GameManager>().isNetworked)
        {
            chatBox.SetActive(true);
        }

        playerNum = PlayerManager.instance.GetIndex(gameObject.transform.parent.gameObject) + 1;

        //Debug.Log("Player: " + PlayerManager.instance.GetIndex(gameObject.transform.parent.gameObject) + 1);
        SetRebindButtons(playerNum);

        //Debug.Log("Count:" + inputInfoList.Count);

        for (int i = 0; i < inputInfoList.Count; i++)
        {
            bindingTexts.Add(inputInfoList[i].actionButton.transform.Find("ControlText").GetComponent<TMP_Text>());
            rebindTextObjects.Add(inputInfoList[i].actionButton.transform.Find("ControlText").gameObject);
            waitingTextObjects.Add(inputInfoList[i].actionButton.transform.Find("InputText").gameObject);

            //Debug.Log("i: " + i);

            int bindingIndex = inputInfoList[i].actionReference.GetBindingIndexForControl(inputInfoList[i].actionReference.controls[0]);

            bindingTexts[bindingTexts.Count - 1].text = InputControlPath.ToHumanReadableString(inputInfoList[i].actionReference.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }



        if (playerNum == 1)
        {
            zoomedInSensitivity.value = StateVariables.p1zoomedInSens;
            zoomedOutSensitivity.value = StateVariables.p1zoomedOutSens;
        }

        else if (playerNum == 2)
        {
            zoomedInSensitivity.value = StateVariables.p2zoomedInSens;
            zoomedOutSensitivity.value = StateVariables.p2zoomedOutSens;
        }

        else if (playerNum == 3)
        {
            zoomedInSensitivity.value = StateVariables.p3zoomedInSens;
            zoomedOutSensitivity.value = StateVariables.p3zoomedOutSens;
        }

        else if (playerNum == 4)
        {
            zoomedInSensitivity.value = StateVariables.p4zoomedInSens;
            zoomedOutSensitivity.value = StateVariables.p4zoomedOutSens;
        }

        else
            Debug.Log("******PROBLEM HERE******");

        GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("UI/Navigate").Disable();
        GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("UI/Submit").Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerNum == 1)
        {
            StateVariables.p1zoomedInSens = zoomedInSensitivity.value;
            StateVariables.p1zoomedOutSens = zoomedOutSensitivity.value;
        }

        else if (playerNum == 2)
        {
            StateVariables.p2zoomedInSens = zoomedInSensitivity.value;
            StateVariables.p2zoomedOutSens = zoomedOutSensitivity.value;
        }

        else if (playerNum == 3)
        {
            StateVariables.p3zoomedInSens = zoomedInSensitivity.value;
            StateVariables.p3zoomedOutSens = zoomedOutSensitivity.value;
        }

        else if (playerNum == 4)
        {
            StateVariables.p4zoomedInSens = zoomedInSensitivity.value;
            StateVariables.p4zoomedOutSens = zoomedOutSensitivity.value;
        }

        rebindTime += Time.deltaTime;
    }

    public void OnPause()
    {
        paused = !paused;
        pauseScreen.SetActive(paused);

        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            //settingsFirst.GetComponent<Button>().Select();

            GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("UI/Navigate").Enable();
            GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("UI/Submit").Enable();
        }

        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("UI/Navigate").Disable();
            GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("UI/Submit").Disable();
        }
    }

    public bool GetIsPaused()
    {
        return paused;
    }

    public void StartRebinding(int index)
    {
        if (rebindTime >= 0.5f)
        {
            //Hide the control text and show the waiting for input text
            rebindTextObjects[index].SetActive(false);
            waitingTextObjects[index].SetActive(true);

            inputInfoList[index].actionReference.Disable();

            rebindingOperation = inputInfoList[index].actionReference.PerformInteractiveRebinding()
                .OnComplete(operation => RebindComplete(index)).Start();
        }
    }

    private void RebindComplete(int index)
    {
        int bindingIndex = inputInfoList[index].actionReference.GetBindingIndexForControl(inputInfoList[index].actionReference.controls[0]);

        bindingTexts[index].text = InputControlPath.ToHumanReadableString(inputInfoList[index].actionReference.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        rebindingOperation.Dispose();

        inputInfoList[index].actionReference.Enable();

        rebindTextObjects[index].SetActive(true);
        waitingTextObjects[index].SetActive(false);

        rebindTime = 0f;
    }

    private void SetRebindButtons(int playerNum)
    {
        for (int i = 0; i < inputInfoList.Count; i++)
        {
            var tempInfo = inputInfoList[i];

            if (tempInfo.actionButton.name.Equals("JumpRebind(0)"))
            {
                tempInfo.actionReference = GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("Player/Jump");
            }

            else if (tempInfo.actionButton.name.Equals("AimRebind(1)"))
            {
                tempInfo.actionReference = GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("Player/Aim");
            }

            else if (tempInfo.actionButton.name.Equals("ThrowRebind(2)"))
            {
                tempInfo.actionReference = GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("Player/Fire");
            }

            else if (tempInfo.actionButton.name.Equals("UpRebind(3)"))
            {
                tempInfo.actionReference = GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("Player/Up");
            }

            else if (tempInfo.actionButton.name.Equals("DownRebind(4)"))
            {
                tempInfo.actionReference = GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("Player/Down");
            }

            else if (tempInfo.actionButton.name.Equals("LeftRebind(5)"))
            {
                tempInfo.actionReference = GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("Player/Left");
            }

            else if (tempInfo.actionButton.name.Equals("RightRebind(6)"))
            {
                tempInfo.actionReference = GameObject.Find("Player" + playerNum).GetComponent<PlayerInput>().actions.FindAction("Player/Right");
            }

            else
                Debug.Log("Problem here!!!!!");

            inputInfoList[i] = tempInfo;
        }
    }
}
