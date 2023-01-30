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
    
    [SerializeField] TransparencyUI onScreenPowerupIcon;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        EventManager.onRoundStart += roundStart;
        EventManager.onRoundEnd += roundEnd;
        hideIOnScreenPowerUpIcon();
        clearText();
    }

    public TransparencyUI getOnScreenPowerUpIcon()
    {
        return onScreenPowerupIcon;
    }

    public void hideIOnScreenPowerUpIcon()
    {
        //onScreenPowerupIcon.enabled = false;
        onScreenPowerupIcon.snapTransparency(0);
    }

    public void showIOnScreenPowerUpIcon()
    {
        onScreenPowerupIcon.snapTransparency(1);
        //onScreenPowerupIcon.enabled = true;

    }

    public void roundStart(object sender, RoundArgs e)
    {
        if(e.getRound(0) == roundType.NONE)
        {
            hideIOnScreenPowerUpIcon();
        }
        
    }

    public void roundEnd(object sender, RoundArgs e)
    {
        if (e.getRound(0) == roundType.NONE)
        {
            showIOnScreenPowerUpIcon();
        }
      
    }


    public void setPowerUpIconImage(Texture image, float lengthOfPowerUp)
    {
        if(image == null && onScreenPowerupIcon.getWantedTransparency() < 0)
        {
            hideIOnScreenPowerUpIcon();
            return;
        }
        //showIOnScreenPowerUpIcon();
        onScreenPowerupIcon.queueFadeTransparency(1f, 0.5f);
        onScreenPowerupIcon.setTransparencyImage(image);
    }

    public void setPowerUpIconImageByPowerUpType(powerUpList type, float length)
    {
        setPowerUpIconImage(getPowerUpIconByType(type),length);
    }

    public Texture getPowerUpIconByType(powerUpList type)
    {
        if(type == powerUpList.None)
        {
            return null;
        }
        //Debug.Log(type.ToString() + " " + ((int)type).ToString() + " " + ((int)type - 1).ToString());
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
