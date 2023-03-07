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

    [SerializeField] GameObject introText;
    [SerializeField] GameObject introCam;

    private PlayerInputManager playerInManager;

    private GameObject p1Ui;
    private GameObject p2Ui;
    private GameObject p3Ui;
    private GameObject p4Ui;

    private List<GameObject> players = new List<GameObject>();

    int numPlayers = 0;

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
        numPlayers++;
        //Debug.Log(playerInput);

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

        Debug.Log(playerInputs.Count);
        
        if (playerInputs.Count == 1)
        {
            playerInput.gameObject.name = "Player1";

            players.Add(playerInput.gameObject);

            p1Ui.SetActive(true);
            p1Ui.GetComponent<Canvas>().worldCamera = playerInput.transform.Find("Main Camera").GetChild(0).GetComponent<Camera>();
            p1Ui.GetComponent<Canvas>().planeDistance = 1f;

            introText.SetActive(false);
            introCam.SetActive(false);

            GameManager.instance.powerupManager.Add(playerInput.gameObject.GetComponent<PowerUpScript>());

            playerInput.gameObject.transform.Find("PlayerObj/EGGROBOT/Character").gameObject.layer = LayerMask.NameToLayer("Player1Model");
            playerInput.gameObject.transform.Find("PlayerObj/EGGROBOT/CharacterTransparent").gameObject.layer = LayerMask.NameToLayer("Player1Transparent");
            playerInput.gameObject.transform.Find("PlayerObj").gameObject.layer = LayerMask.NameToLayer("Player1");

        }

        else if (playerInputs.Count == 2)
        {
            playerInput.gameObject.name = "Player2";
            playerInput.gameObject.transform.GetComponentInChildren<AudioListener>().enabled = false;

            players.Add(playerInput.gameObject);

            p2Ui.SetActive(true);
            p2Ui.GetComponent<Canvas>().worldCamera = playerInput.transform.Find("Main Camera").GetChild(0).GetComponent<Camera>();
            p2Ui.GetComponent<Canvas>().planeDistance = 1f;

            p1Ui.GetComponent<CanvasScaler>().scaleFactor = 0.5f;
            p2Ui.GetComponent<CanvasScaler>().scaleFactor = 0.5f;

            GameManager.instance.powerupManager.Add(playerInput.gameObject.GetComponent<PowerUpScript>());

            playerInput.gameObject.transform.Find("PlayerObj/EGGROBOT/Character").gameObject.layer = LayerMask.NameToLayer("Player2Model");
            playerInput.gameObject.transform.Find("PlayerObj/EGGROBOT/CharacterTransparent").gameObject.layer = LayerMask.NameToLayer("Player2Transparent");
            playerInput.gameObject.transform.Find("PlayerObj").gameObject.layer = LayerMask.NameToLayer("Player2");
        }

        else if (playerInputs.Count == 3)
        {
            playerInput.gameObject.name = "Player3";
            playerInput.gameObject.transform.GetComponentInChildren<AudioListener>().enabled = false;

            players.Add(playerInput.gameObject);

            p3Ui.SetActive(true);
            p3Ui.GetComponent<Canvas>().worldCamera = playerInput.transform.Find("Main Camera").GetChild(0).GetComponent<Camera>();
            p3Ui.GetComponent<Canvas>().planeDistance = 1f;

            p1Ui.GetComponent<CanvasScaler>().scaleFactor = 1f;
            p2Ui.GetComponent<CanvasScaler>().scaleFactor = 1f;

            GameManager.instance.powerupManager.Add(playerInput.gameObject.GetComponent<PowerUpScript>());

            playerInput.gameObject.transform.Find("PlayerObj/EGGROBOT/Character").gameObject.layer = LayerMask.NameToLayer("Player3Model");
            playerInput.gameObject.transform.Find("PlayerObj/EGGROBOT/CharacterTransparent").gameObject.layer = LayerMask.NameToLayer("Player3Transparent");
            playerInput.gameObject.transform.Find("PlayerObj").gameObject.layer = LayerMask.NameToLayer("Player3");
        }

        else if (playerInputs.Count == 4)
        {
            playerInput.gameObject.name = "Player4";
            playerInput.gameObject.transform.GetComponentInChildren<AudioListener>().enabled = false;

            players.Add(playerInput.gameObject);

            p4Ui.SetActive(true);
            p4Ui.GetComponent<Canvas>().worldCamera = playerInput.transform.Find("Main Camera").GetChild(0).GetComponent<Camera>();
            p4Ui.GetComponent<Canvas>().planeDistance = 1f;

            p4Ui.GetComponent<CanvasScaler>().scaleFactor = 1f;

            GameManager.instance.powerupManager.Add(playerInput.gameObject.GetComponent<PowerUpScript>());

            playerInput.gameObject.transform.Find("PlayerObj/EGGROBOT/Character").gameObject.layer = LayerMask.NameToLayer("Player4Model");
            playerInput.gameObject.transform.Find("PlayerObj/EGGROBOT/CharacterTransparent").gameObject.layer = LayerMask.NameToLayer("Player4Transparent");
            playerInput.gameObject.transform.Find("PlayerObj").gameObject.layer = LayerMask.NameToLayer("Player4");
        }
    }

    public GameObject GetPlayer(int playerNum)
    {
        return players[playerNum - 1];
    }

    public int GetIndex(GameObject playerObject)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == playerObject)
            {
                return i;
            }
        }

        return -1;
    }

    public int GetNumPlayers()
    {
        return numPlayers;
    }
}
