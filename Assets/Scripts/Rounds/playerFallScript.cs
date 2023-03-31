using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerFallScript : MonoBehaviour
{
    [SerializeField] bool debugPrint;
    public static playerFallScript instance;
    public List<GameObject> fallenPlayers = new List<GameObject>();
    public bool checkCollision = true;
   

    public void resetFallenPlayers()
    {
        fallenPlayers.Clear();
    }

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!checkCollision)
        {
            return;
        }
        if(collision.gameObject.tag.Equals("Player"))
        {
            if (fallenPlayers.Contains(collision.gameObject))
            {
                return;
            }
            fallenPlayers.Add(collision.gameObject);
            if (debugPrint) {
                Debug.Log("$COLLISION");
            }
            EventManager.onPlayerFell?.Invoke(null, new PlayerArgs(collision.gameObject));
        }
        else if (collision.gameObject.tag.Equals("Bullet"))
        {
            collision.gameObject.GetComponent<BallBehaviour>().ResetBall();
        }
    }

    
}
