using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CardUI : MonoBehaviour
{

    public RectTransform startTrans;
    public RectTransform endTrans;
    float T = -1f;
    public float secondsLong;
    public float delayedStart = 0f;
    float secondsElapsed = 0f;
    public movementType movement;
    

    void OnGUI()
    {
       
    }

    // Start is called before the first frame update
    void Start()
    {
        setTransform(startTrans);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(delayedStart >= 0f)
        {
            secondsElapsed += Time.deltaTime;
            if (secondsElapsed > delayedStart)
            {
                delayedStart = -1f;
                startMove();
            }
        }
        
        if(T >= 0)
        {
            T += Time.deltaTime / secondsLong;
            float newT = getNewT();
            transform.localScale = Vector3.Lerp(startTrans.localScale, endTrans.localScale, newT);
            transform.rotation = Quaternion.Slerp(startTrans.rotation, endTrans.rotation, newT);
            transform.position = Vector3.Lerp(startTrans.position, endTrans.position, newT);
            if(T > 1)
            {
                setTransform(endTrans);
                T = -1;
            }
        }
    }

    public void setTransform(RectTransform r)
    {
        transform.localScale = r.localScale;
        transform.rotation = r.rotation;
        transform.position = r.position;
    }

    public void startMove()
    {
        T = 0f;
    }

    public float getNewT()
    {
        if(movement == movementType.LERP)
        {
            return T;
        }
        if (movement == movementType.EASEIN)
        {
            return 1 - Mathf.Cos((T * Mathf.PI) / 2);
        }
        if (movement == movementType.EASEOUT)
        {
            return Mathf.Sin((T * Mathf.PI) / 2);
        }
        return T;
    }
}

public enum movementType
{
    LERP,
    EASEIN,
    EASEOUT
}
