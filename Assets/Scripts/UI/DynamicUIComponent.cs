using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicUIComponent : MonoBehaviour
{
    Transform movingPos = null;
    Transform startingPos = null;
    float T = 0f;
    float secondsToMove = -1f;
    public float delayedStart = 0f;
    float timeUntilStart = 0f;
    bool hasEnded = false;
   // bool shouldRetract = false;
    float timeOn = -1f;
    public float timeUntilEnding = -1;
    [SerializeField] protected Transform UIOnScreen;
    [SerializeField] protected Transform UIOffScreen;
    public bool doRotations = false;
    public bool repeatEnd = false;
    int timesCompleted = 0;
    float totalRunTime = 0;
    public EventHandler<EventArgs> onEnd;

    // Start is called before the first frame update
    void Start()
    {
        onStart();
    }

    // Update is called once per frame
    void Update()
    {
        
        checkForMovement();   
    }

    public virtual void checkForMovement()
    {
        if (movingPos != null && secondsToMove > 0)
        {
            totalRunTime += Time.deltaTime;
            if (delayedStart > timeUntilStart)
            {
                timeUntilStart += Time.deltaTime;
                return;
            }
            
            
            if (T > 1)
            {
                /*
                if (!hasEnded)
                {
                    if(timeUntilEnding >= 0)
                    {
                        timeOn += Time.deltaTime;
                        if(timeOn < timeUntilEnding)
                        {
                            return;
                        }
                        else
                        {
                            shouldRetract = true;
                        }
                    }
                    if (shouldRetract)
                    {
                        hasEnded = true;
                        EndToStart(secondsToMove);
                        return;
                    }
                }
                */
                
                
                
                
                
                    if (repeatEnd)
                    {
                    if(timeUntilEnding != -1)
                    {
                        if (totalRunTime >= timeUntilEnding)
                        {
                            onEnd?.Invoke(null, System.EventArgs.Empty);
                            repeatEnd = false;
                            EndToStart(secondsToMove);
                           
                        }
                    }

                    return;
                    }
                
                T = 0;
                movingPos = null;
                startingPos = null;
                secondsToMove = -1;
                return;
            }
            T += Time.deltaTime / secondsToMove;
            transform.position = Vector3.Lerp(startingPos.position, movingPos.position, ease(T));
            if (doRotations)
            {
                transform.rotation = Quaternion.Lerp(startingPos.rotation, movingPos.rotation, ease(T));
            }
        }
    }

    public float ease(float T)
    {
        return Mathf.Sin((T * Mathf.PI) / 2f);
    }

    public virtual void onStart()
    {
        //Debug.Log(gameObject.transform.parent.name);
        transform.position = UIOffScreen.position;
    }

    public void StartToEnd(float secondsLong)
    {
        easeIn(UIOnScreen, secondsLong, UIOffScreen);
    }

    public void EndToStart(float secondsLong)
    {
        easeIn(UIOffScreen, secondsLong, UIOnScreen);
    }

    public void easeIn(Transform t, float secondsLong, Transform starting = null)
    {
        if(starting != null)
        {
            transform.position = starting.position;
        }
        startingPos = transform;
        secondsToMove = secondsLong;
        T = 0;
        movingPos = t;
    }
}
