using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupGiver : MonoBehaviour
{
    public powerUpList type;

    public void setPowerup(powerUpList t)
    {
        type = t;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(type == powerUpList.None)
        {
            return;
        }
        if (other.gameObject.tag.Equals("Player"))
        {
            //Debug.Log(other.gameObject.name);
            if(other.transform.parent.gameObject.GetComponent<PowerUpScript>() != null)
            {
                if (other.transform.parent.gameObject.GetComponent<PowerUpScript>().getSelectedPowerUp() != powerUpList.None)
                {
                    UIManager.instance.setText("You already have a powerup! " + other.transform.parent.gameObject.GetComponent<PowerUpScript>().getSelectedPowerUp().ToString()); ;
                    return;
                }
                    other.transform.parent.gameObject.GetComponent<PowerUpScript>().setSelectedPowerUp(type);
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
