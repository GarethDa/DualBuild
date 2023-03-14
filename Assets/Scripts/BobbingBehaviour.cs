using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobbingBehaviour : MonoBehaviour
{
    public float heightDifferenceUp = 1;
    //public float heightDifferenceDown = 0;
    public float secondsGoingUp = 1;
    public float delayedStart = 0;
    public bool simpleEase = false;
    //public float secondsGoingDown = 1;
    float t = 0;
    float multiplier = 1;
    //bool goingUp = true;
    Vector3 oldPos;
    // Start is called before the first frame update
    void Start()
    {
        oldPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(delayedStart > 0)
        {
            delayedStart -= Time.deltaTime;
            return;
        }
        t += (Time.deltaTime/ secondsGoingUp) * multiplier;
       // Debug.Log((secondsGoingUp * Time.deltaTime) * multiplier);
        if(t > 1 || t < 0)
        {
            if (t > 1)
            {
                t = 1;
            }
            if (t < 0)
            {
                t = 0;
            }
            multiplier *= -1;
            
        }
       // Debug.Log(ease(t));
        transform.position = Lerp(oldPos, oldPos + (Vector3.up * heightDifferenceUp),ease(t));

    }

    private Vector3 Lerp(Vector3 v1, Vector3 v2, float t)//unity clamps T to >=0 && <= 1
    {
        float x = v1.x + ((v2.x - v1.x) * t);
        float y = v1.y + ((v2.y - v1.y) * t);
        float z = v1.z + ((v2.z - v1.z) * t);
        return new Vector3(x, y, z);
    }

    float ease(float x)
    {
        if (simpleEase)
        {
            return Mathf.Sin((x * Mathf.PI) / 2);
        }
        //return x;
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;

        return x < 0.5
          ? (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
          : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
    }
}
