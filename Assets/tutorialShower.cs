using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutorialShower : MonoBehaviour
{
    public inWorldTutorial tutorial;
    List<GameObject> closestObjects = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.GetComponent<BallBehaviour>() != null)
        {
            if (transform.parent.GetComponent<CharacterAiming>().getHeldProjectile() == other.gameObject)
            {
                hideTutorial();
                return;
            }
            if (transform.parent.GetComponent<CharacterAiming>().IsHoldingProj())
            {
                hideTutorial();
                return;
            }
            BallBehaviour ball = other.GetComponent<BallBehaviour>();
            if(!ball.GetIsHeld() && !ball.GetIsThrown())
            {
                closestObjects.Add(other.gameObject);
                updateClosest();
            }
            
        }
    }

    public void hideTutorial()
    {
        tutorial.hide();
    }
    private void updateClosest()
    {
        GameObject closest = null;
        float minDist = float.MaxValue;
        foreach(GameObject g in closestObjects)
        {
            float dist = Vector3.Distance(transform.position, g.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = g;
               
            }
        }
        if(closest == null)
        {
            return;
        }
        //do check to change what gets shown
        tutorial.show(closest.transform, "Pickup dodgeball");
    }

    private void OnTriggerExit(Collider other)
    {
        if (closestObjects.Contains(other.gameObject))
        {
            closestObjects.Remove(other.gameObject);
        }
        if(closestObjects.Count == 0)
        {
            tutorial.hide();
        }
        else
        {
            if (transform.parent.GetComponent<CharacterAiming>().IsHoldingProj())
            {
                hideTutorial();
                return;
            }
            if(other.gameObject.GetComponent<BallBehaviour>() != null)
            {
                if (other.gameObject.GetComponent<BallBehaviour>().GetIsThrown())
                {
                    hideTutorial();
                    return;
                }
            }
            updateClosest();
        }
    }
}
