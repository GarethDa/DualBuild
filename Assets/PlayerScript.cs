using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    PhotonView view;
    void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        RoundManager.instance.currentPlayers.Add(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
