using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicUIComponent : MonoBehaviour
{
    Transform movingPos = null;
    Transform startingPos = null;
    float T = 0f;
    float secondsToMove = -1f;
   
    
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
            if (T > 1)
            {
                movingPos = null;
                startingPos = null;
                T = 0;
                secondsToMove = -1;
                return;
            }
            T += Time.deltaTime / secondsToMove;
            transform.position = Vector3.Lerp(startingPos.position, movingPos.position, ease(T));
        }
    }

    public float ease(float T)
    {
        return Mathf.Sin((T * Mathf.PI) / 2f);
    }

    public virtual void onStart()
    {
        transform.position = UIManager.instance.UIOffScreen.position;
    }

    public void easeIn(Transform t, float secondsLong, Transform starting = null)
    {
        if(starting != null)
        {
            transform.position = starting.position;
        }
        startingPos = transform;
        secondsToMove = secondsLong;
        movingPos = t;
    }
}
