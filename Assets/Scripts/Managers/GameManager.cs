using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public GameObject playerManager;
    public GameObject levelManager;
    public GameObject deathZone;
    public List<PowerUpScript> powerupManager = new List<PowerUpScript>();
    public GameObject clientPlayer;

    bool isNetworked = false;
    bool isHost = false;

    public int playersConnected = 1;
    
    void Awake()
    {
        if(instance == null)
        {
            instance = this;

        }
    }

    public static GameObject createGameObject(string prefab)
    {
        Debug.Log(prefab);
        return Instantiate<GameObject>(Resources.Load<GameObject>(prefab));
    }


}
