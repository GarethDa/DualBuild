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
    [SerializeField] List<Texture> powerupIcons;
    [SerializeField] RawImage onScreenPowerupIcon;
    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        hideIOnScreenPowerUpIcon();
        clearText();
    }

    public void hideIOnScreenPowerUpIcon()
    {
        onScreenPowerupIcon.enabled = false;
    }

    public void showIOnScreenPowerUpIcon()
    {
        onScreenPowerupIcon.enabled = true;

    }

    public void setPowerUpIconImage(Texture image)
    {
        if(image == null)
        {
            hideIOnScreenPowerUpIcon();
            return;
        }
        showIOnScreenPowerUpIcon();
        onScreenPowerupIcon.texture = image;
    }

    public Texture getPowerUpIconByType(powerUpList type)
    {
        if(type == powerUpList.None)
        {
            return null;
        }
        Debug.Log(type.ToString() + " " + ((int)type).ToString() + " " + ((int)type - 1).ToString());
        return powerupIcons[((int)type)-1];
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
