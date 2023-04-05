using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class inWorldTutorial : MonoBehaviour
{
   
    string text = "";
    Transform placeToShow;
    public TMP_Text textObject;
    public Camera toLook;
    public float height = 1.5f;
    public void show(Transform location, string tutorialText)
    {
       
        placeToShow = location;
        text = tutorialText;
       
        setText();
    }

    public void hide()
    {
        text = "";
        setText();

    }

    public void setText()
    {
        
        textObject.text = text;   
    }

    private void Update()
    {
        if(text.Length == 0)
        {
            return;
        }
        if(placeToShow == null)
        {
            return;
        }
        //Debug.Log(toLook.transform.position);
        transform.position = placeToShow.transform.position + (Vector3.up * height);
        gameObject.transform.forward = ( transform.position- toLook.transform.position);


        
        //gameObject.transform.rotation = Quaternion.Inverse(transform.rotation);
    }
}
