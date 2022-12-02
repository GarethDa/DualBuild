using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    protected int playersDied = 0;

    private List<PlayerInput> playerInputs = new List<PlayerInput>();

    [SerializeField] private List<Transform> spawnPoints;

    [SerializeField] private List<LayerMask> playerLayers;

    private PlayerInputManager playerInManager;
    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        playerInManager = FindObjectOfType<PlayerInputManager>();
    }

    private void OnEnable()
    {
        playerInManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        playerInManager.onPlayerJoined -= AddPlayer;
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
        playerInputs.Add(playerInput);

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
        
        playerInput.transform.parent = GameObject.Find("PlayerManager").transform;

        if (playerInputs.Count == 1)
        {
            GameObject.Find("P1_UI").GetComponent<Canvas>().worldCamera = playerInput.transform.GetComponentInChildren<Camera>();
            GameObject.Find("P1_UI").GetComponent<Canvas>().planeDistance = 1;
        }

        else if (playerInputs.Count == 2)
        {
            GameObject.Find("P2_UI").GetComponent<Canvas>().worldCamera = playerInput.transform.GetComponentInChildren<Camera>();
            GameObject.Find("P2_UI").GetComponent<Canvas>().planeDistance = 1;
        }

    }

}
