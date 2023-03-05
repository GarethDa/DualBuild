using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    protected int playersDied = 0;

    private List<PlayerInput> playerInputs = new List<PlayerInput>();

    [SerializeField] private List<Transform> spawnPoints;

    [SerializeField] private List<LayerMask> playerLayers;

    private PlayerInputManager playerInManager;

    private GameObject p1Ui;
    private GameObject p2Ui;
    private GameObject p3Ui;
    private GameObject p4Ui;

    private List<GameObject> players = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        if (GameObject.Find("P1_UI") != null)
        {
            p1Ui = GameObject.Find("P1_UI");
            p2Ui = GameObject.Find("P2_UI");
            p3Ui = GameObject.Find("P3_UI");
            p4Ui = GameObject.Find("P4_UI");
            p1Ui.SetActive(false);
            p2Ui.SetActive(false);
            p3Ui.SetActive(false);
            p4Ui.SetActive(false);
        }

        if (instance == null)
        {
            instance = this;
        }

        //playerInManager = FindObjectOfType<PlayerInputManager>();
    }

    private void OnEnable()
    {
        //playerInManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        //playerInManager.onPlayerJoined -= AddPlayer;
    }

    protected void checkLastOneStanding()
    {
        if (playersDied == GameManager.instance.playersConnected - 1)
        {
            RoundManager.instance.endRound("playersConnected = 1");
        }
    }

    public void playerDied(GameObject p)
    {
        playersDied++;
        EventManager.onPlayerFell?.Invoke(null, new PlayerArgs(p));
        checkLastOneStanding();
    }

    public int getPlayersDied()
    {
        return playersDied;
    }

    public void resetPlayerDeaths()
    {
        playersDied = 0;
    }

    public void AddPlayer(PlayerInput playerInput)
    {
        Debug.Log(playerInput);

        playerInputs.Add(playerInput);

        playerInput.transform.parent = GameObject.Find("PlayerHolder").transform;

        //Set the player's position
        playerInput.transform.position = spawnPoints[playerInputs.Count - 1].position;

        //Convert the layer mask to an int
        int layerInt = (int)Mathf.Log(playerLayers[playerInputs.Count - 1].value, 2);

        //Set the layer
        playerInput.transform.GetComponentsInChildren<CinemachineVirtualCamera>()[0].gameObject.layer = layerInt;
        playerInput.transform.GetComponentsInChildren<CinemachineVirtualCamera>()[1].gameObject.layer = layerInt;

        //Add the layer
        playerInput.transform.GetComponentInChildren<Camera>().cullingMask |= 1 << layerInt;

        //Set the action in the custom cinemachine input script
        playerInput.transform.GetComponentsInChildren<CMachineInput>()[0].horizontal = playerInput.actions.FindAction("Look");
        playerInput.transform.GetComponentsInChildren<CMachineInput>()[1].horizontal = playerInput.actions.FindAction("Look");
        
        if (playerInputs.Count == 1)
        {
            p1Ui.SetActive(true);
            p1Ui.GetComponent<Canvas>().worldCamera = playerInput.transform.GetComponentInChildren<Camera>();
            p1Ui.GetComponent<Canvas>().planeDistance = 0.5f;

            playerInput.gameObject.name = "Player1";

            players.Add(playerInput.gameObject);
        }

        else if (playerInputs.Count == 2)
        {
            p2Ui.SetActive(true);
            p2Ui.GetComponent<Canvas>().worldCamera = playerInput.transform.GetComponentInChildren<Camera>();
            p2Ui.GetComponent<Canvas>().planeDistance = 0.5f;

            playerInput.gameObject.name = "Player2";
            playerInput.gameObject.transform.GetComponentInChildren<AudioListener>().enabled = false;

            players.Add(playerInput.gameObject);
        }

        else if (playerInputs.Count == 3)
        {
            p3Ui.SetActive(true);
            p3Ui.GetComponent<Canvas>().worldCamera = playerInput.transform.GetComponentInChildren<Camera>();
            p3Ui.GetComponent<Canvas>().planeDistance = 0.5f;

            playerInput.gameObject.name = "Player3";
            playerInput.gameObject.transform.GetComponentInChildren<AudioListener>().enabled = false;

            players.Add(playerInput.gameObject);
        }

        else if (playerInputs.Count == 4)
        {
            p4Ui.SetActive(true);
            p4Ui.GetComponent<Canvas>().worldCamera = playerInput.transform.GetComponentInChildren<Camera>();
            p4Ui.GetComponent<Canvas>().planeDistance = 0.5f;

            playerInput.gameObject.name = "Player4";
            playerInput.gameObject.transform.GetComponentInChildren<AudioListener>().enabled = false;

            players.Add(playerInput.gameObject);
        }
    }

    public GameObject GetPlayer(int playerNum)
    {
        Debug.Log(playerNum);
        return players[playerNum - 1];
    }
}
