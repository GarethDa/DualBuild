using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutorialShower : MonoBehaviour
{
    public inWorldTutorial tutorial;
    List<GameObject> closestObjects = new List<GameObject>();
    int timesShownPlayer = 0;

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
            bool isParentPlayer = false;

            if(other.transform.parent.gameObject.layer== 9 || other.transform.parent.gameObject.layer == 8)
            {
                isParentPlayer = true;
            }
            if (isParentPlayer)
            {
                hideTutorial();
                return;
            }
            if(!ball.GetIsHeld() && !ball.GetIsThrown() )
            {
                closestObjects.Add(other.gameObject);
                updateClosest();
            }
            else
            {
                hideTutorial();
                return;
            }
            
            
        }

        if (other.gameObject.tag.Contains("Player"))
        {
            closestObjects.Add(other.gameObject);
            updateClosest();
        }
    }

    public void hideTutorial()
    {
        tutorial.hide();
    }
    public void updateClosest()
    {
        GameObject closest = null;
        float minDist = float.MaxValue;
        List<GameObject> toRemove = new List<GameObject>();
        foreach(GameObject g in closestObjects)
        {
            if(g == null)
            {
                toRemove.Add(g);
                continue;
            }
            float dist = Vector3.Distance(transform.position, g.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = g;
               
            }
        }
        foreach(GameObject g in toRemove)
        {
            closestObjects.Remove(g);
        }
        if(closest == null)
        {
            return;
        }
        //do check to change what gets shown
        if (closest.tag.Contains("Player") )
        {
            if(timesShownPlayer < 2)
            {
                tutorial.height = 4.5f;
                tutorial.show(closest.transform, "Press " + GameManager.instance.getButtonString("Player/Fire", transform.parent.gameObject) + " to punch");
                timesShownPlayer++;
            }
            
        }
        else if (closest.tag.Contains("Bullet"))
        {
            bool isParentPlayer = false;
            if(closest.transform.parent != null)
            {
                if (closest.transform.parent.gameObject.layer == 9 || closest.transform.parent.gameObject.layer == 8)
                {
                    isParentPlayer = true;
                }
            }
            
            if (isParentPlayer)
            {
                hideTutorial();
                return;
            }
            if (!closest.GetComponent<BallBehaviour>().GetIsHeld() && !closest.GetComponent<BallBehaviour>().GetIsThrown())
            {
                tutorial.height = 1.5f;
                tutorial.show(closest.transform, "Pickup dodgeball");
            }
            else
            {
                hideTutorial();
                return;
            }
            

        }
        else
        {
            hideTutorial();
        }
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
