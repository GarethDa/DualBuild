using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerFallScript : MonoBehaviour
{
    public static playerFallScript instance;
    public List<GameObject> fallenPlayers = new List<GameObject>();
   

    public void resetFallenPlayers()
    {
        fallenPlayers.Clear();
    }
    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag.Equals("Player"))
        {
            if (fallenPlayers.Contains(collision.gameObject))
            {
                return;
            }
            fallenPlayers.Add(collision.gameObject);
            //its a player
            Debug.Log("$COLLISION");
            //playerDied();
            EventManager.onPlayerFell?.Invoke(null, new PlayerArgs(collision.gameObject));
            //PlayerManager.instance.playerfell(collision.gameObject);
           // Debug.Log("FELL");
        }
    }

    
}
