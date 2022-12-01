using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;
    public Transform UIOffScreen;
    public Transform UIOnScreen;
    public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        clearText();
    }

    public void setText(string s)
    {
        text.text = s;
    }
    public void clearText()
    {
        setText("");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
