using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupGiver : MonoBehaviour
{
    public powerUpList type;
    // Start is called before the first frame update
    void Start()
    {
        
    }
  
    // Update is called once per frame
    void Update()
    {
     
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            //Debug.Log(other.gameObject.name);
            if(other.transform.parent.gameObject.GetComponent<PowerUpScript>() != null)
            {
                other.transform.parent.gameObject.GetComponent<PowerUpScript>().selectedPowerUp = type;
                UIManager.instance.setText("Active powerup: " + type.ToString());
            }
            
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            //other.gameObject.GetComponent<PowerUpScript>().selectedPowerUp = type;
            UIManager.instance.clearText();
        }
    }
}
