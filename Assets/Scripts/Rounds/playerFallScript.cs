using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerFallScript : MonoBehaviour
{
    public static playerFallScript instance;
   
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
            //its a player
            Debug.Log("COLLISION");
            //playerDied();
            EventManager.onPlayerFell?.Invoke(null, new PlayerArgs(collision.gameObject));
            //PlayerManager.instance.playerfell(collision.gameObject);
           // Debug.Log("FELL");
        }
    }

    
}
