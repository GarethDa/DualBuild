using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CardUI : TransformInterpolator
{

    
    

    // Start is called before the first frame update
    void Start()
    {
        setTransform(edges[0].transform);
        startMove();
        
    }

    // Update is called once per frame
    void Update()
    {
        

        move();
    }

    
}


