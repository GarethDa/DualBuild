using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

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
   
    [SerializeField] private string singlePlayer;
    [SerializeField] private string multiPlayer;
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject settingsScreen;

    [Header("Control rebinding")]
    [SerializeField] private List<InputInfoStruct> inputInfoList;

    [Header("Sliders")]
    [SerializeField] private Slider zoomedInSensitivity;
    [SerializeField] private Slider zoomedOutSensitivity;
    [SerializeField] private Slider musicVolume;
    //Add SFX Slider if possible

    [Header("First highlighted buttons")]
    [SerializeField] GameObject mainMenuFirst;
    [SerializeField] GameObject settingsFirst;

    [Header("Audio")]
    public AudioSource audioSource;

    //[SerializeField] private PlayerInput pInput;
    //A list of texts which display the current binding of each action
    private List<TMP_Text> bindingTexts = new List<TMP_Text>();

    //A list of the same text's game objects
    private List<GameObject> rebindTextObjects = new List<GameObject>();

    //A list of the text which appears to tell the player that the system is waiting for an input
    private List<GameObject> waitingTextObjects = new List<GameObject>();

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private float rebindTime = 0f;

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

        zoomedInSensitivity.value = StateVariables.p1zoomedInSens;
        zoomedOutSensitivity.value = StateVariables.p1zoomedOutSens;
        musicVolume.value = StateVariables.musicVar;

        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        StateVariables.p1zoomedInSens = zoomedInSensitivity.value;
        StateVariables.p1zoomedOutSens = zoomedOutSensitivity.value;
        StateVariables.musicVar = musicVolume.value;

        audioSource.volume = (musicVolume.value / 100);

        rebindTime += Time.deltaTime;
    }

    public void StartSingleplayer()
    {
        SceneManager.LoadScene(singlePlayer);
    }

    public void StartMultiplayer()
    {
        SceneManager.LoadScene(multiPlayer);
    }

    public void OpenOptions()
    {
        settingsScreen.SetActive(true);
        mainScreen.SetActive(false);

        settingsFirst.GetComponent<Button>().Select();
    }

    public void CloseOptions()
    {
        settingsScreen.SetActive(false);
        mainScreen.SetActive(true);

        mainMenuFirst.GetComponent<Button>().Select();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }

    public void StartRebinding(int index)
    {
        if (rebindTime >= 0.5f)
        {
            //Hide the control text and show the waiting for input text
            rebindTextObjects[index].SetActive(false);
            waitingTextObjects[index].SetActive(true);

            inputInfoList[index].actionReference.action.Disable();

            rebindingOperation = inputInfoList[index].actionReference.action.PerformInteractiveRebinding().OnComplete(operation => RebindComplete(index)).Start();
        }
        
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

        rebindTime = 0f;
    }
}
