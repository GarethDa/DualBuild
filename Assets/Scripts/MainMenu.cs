using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class MainMenu : MonoBehaviour
{
    //A struct which holds a reference to an input action and the button which will rebind it
    [Serializable]
    private struct InputInfoStruct
    {
        public InputActionReference actionReference;
        public GameObject actionButton;
    }

    [Header("Scene/Screen switching")]
    [SerializeField] private string singleplayer;
    [SerializeField] private string multiplayer;
    [SerializeField] private GameObject settingsScreen;

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

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < inputInfoList.Count; i++)
        {
            bindingTexts.Add(inputInfoList[i].actionButton.transform.Find("ControlText").GetComponent<TMP_Text>());
            rebindTextObjects.Add(inputInfoList[i].actionButton.transform.Find("ControlText").gameObject);
            waitingTextObjects.Add(inputInfoList[i].actionButton.transform.Find("InputText").gameObject);
        }

        zoomedInSensitivity.value = StateVariables.zoomedInSens;
        zoomedOutSensitivity.value = StateVariables.zoomedOutSens;
    }

    // Update is called once per frame
    void Update()
    {
        StateVariables.zoomedInSens = zoomedInSensitivity.value;
        StateVariables.zoomedOutSens = zoomedOutSensitivity.value;
    }

    public void StartSinglePlayer()
    {
        SceneManager.LoadScene(singleplayer);
    }

    public void StartMultiplayer()
    {
        SceneManager.LoadScene(multiplayer);
    }

    public void OpenOptions()
    {
        settingsScreen.SetActive(true);
    }

    public void CloseOptions()
    {
        settingsScreen.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }

    public void StartRebinding(int index)
    {
        //Hide the control text and show the waiting for input text
        rebindTextObjects[index].SetActive(false);
        waitingTextObjects[index].SetActive(true);

        rebindingOperation = inputInfoList[index].actionReference.action.PerformInteractiveRebinding()
            .OnMatchWaitForAnother(0.1f).OnComplete(operation => RebindComplete(index)).Start();
    }

    private void RebindComplete(int index)
    {
        int bindingIndex = inputInfoList[index].actionReference.action.GetBindingIndexForControl(inputInfoList[index].actionReference.action.controls[0]);

        bindingTexts[index].text = InputControlPath.ToHumanReadableString(inputInfoList[index].actionReference.action.bindings[bindingIndex].effectivePath, 
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        rebindingOperation.Dispose();

        rebindTextObjects[index].SetActive(true);
        waitingTextObjects[index].SetActive(false);


    }
}
