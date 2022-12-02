using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public float spawnTime = 1;
    
    public static SpawnPlayers INSTANCE;

    void Awake()
    {
        if (INSTANCE == null)
        {
            INSTANCE = this;
        }
    }

    private void Start()
    {
        SpawnMyPlayer();
    }

    void Update()
    {
        /**
        if (NetworkTimer.time < 0)
            return;

        NetworkTimer.time += Time.deltaTime;

        Debug.Log("NETWORK TIMER");
        if (NetworkTimer.time >= spawnTime)
        {
            SpawnMyPlayer();
        }
        */
    }

    void SpawnMyPlayer()
    {
        Debug.Log("SPAWNING PLAYER");
        Vector3 Pos = transform.position;
        GameObject myPlayer = PhotonNetwork.Instantiate(playerPrefab.name, Pos, Quaternion.identity);
        myPlayer.transform.SetParent(GameManager.instance.playerManager.transform);//please dont remove this

        //ENABLED SO THAT EACH CLIENT HAS THEIR OWN VERSION
        myPlayer.GetComponent<TpMovement>().enabled = true;
        myPlayer.GetComponent<PlayerInput>().enabled = true;
        myPlayer.GetComponent<CharacterAiming>().enabled = true;
        myPlayer.GetComponent<CameraSettings>().enabled = true;
        myPlayer.transform.Find("Camera Holder").gameObject.SetActive(true);
        myPlayer.transform.Find("Zoomed Camera").gameObject.SetActive(true);
        myPlayer.transform.Find("Virtual Camera").gameObject.SetActive(true);

        NetworkTimer.time = -1;
    }

}
