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
        public InputActionReference actionReference;
        public GameObject actionButton;
    }

    [Header("Scene/Screen switching")]
    [SerializeField] private GameObject pauseScreen;

    [Header("Control rebinding")]
    [SerializeField] private List<InputInfoStruct> inputInfoList;

    [Header("Sliders")]
    [SerializeField] private Slider zoomedInSensitivity;
    [SerializeField] private Slider zoomedOutSensitivity;

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

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < inputInfoList.Count; i++)
        {
            bindingTexts.Add(inputInfoList[i].actionButton.transform.Find("ControlText").GetComponent<TMP_Text>());
            rebindTextObjects.Add(inputInfoList[i].actionButton.transform.Find("ControlText").gameObject);
            waitingTextObjects.Add(inputInfoList[i].actionButton.transform.Find("InputText").gameObject);

            int bindingIndex = inputInfoList[i].actionReference.action.GetBindingIndexForControl(inputInfoList[i].actionReference.action.controls[0]);

            bindingTexts[bindingTexts.Count - 1].text = InputControlPath.ToHumanReadableString(inputInfoList[i].actionReference.action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }

        for (int i = 1; i < 5; i++)
        {
            if (gameObject.name.Equals("P" + i + "_UI"))
            {
                playerNum = i;
                break;
            }

            playerNum = -1;
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
    }

    public void OnPause()
    {
        paused = !paused;
        pauseScreen.SetActive(paused);

        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void StartRebinding(int index)
    {
        //Hide the control text and show the waiting for input text
        rebindTextObjects[index].SetActive(false);
        waitingTextObjects[index].SetActive(true);

        inputInfoList[index].actionReference.action.Disable();

        rebindingOperation = inputInfoList[index].actionReference.action.PerformInteractiveRebinding()
            .OnMatchWaitForAnother(0.1f).OnComplete(operation => RebindComplete(index)).Start();
    }

    private void RebindComplete(int index)
    {
        int bindingIndex = inputInfoList[index].actionReference.action.GetBindingIndexForControl(inputInfoList[index].actionReference.action.controls[0]);

        bindingTexts[index].text = InputControlPath.ToHumanReadableString(inputInfoList[index].actionReference.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        rebindingOperation.Dispose();

        inputInfoList[index].actionReference.action.Enable();

        rebindTextObjects[index].SetActive(true);
        waitingTextObjects[index].SetActive(false);


    }
}
