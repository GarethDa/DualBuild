using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwappableUIManager : MonoBehaviour
{
    public SwappableUI[] UIPages;
    public Camera UICam;
    
    public static SwappableUIManager instance;
    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        showUI(UIMenuType.CHOOSE_USERNAME);
    }

    public void showUI(UIMenuType type)
    {
        for(int i = 0; i < UIPages.Length; i++)
        {
            UIPages[i].hide();
        }
        UIPages[(int)type].show();
    }

    public void hideAll()
    {
        for (int i = 0; i < UIPages.Length; i++)
        {
            UIPages[i].hide();
        }
    }

    public SwappableUI getUI(UIMenuType t)
    {
        return UIPages[(int)t];
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum UIMenuType
{
    CHOOSE_USERNAME,
    JOIN_GAME,
    HOST_GAME,
    HOME,
    ENTER_CODE
}
