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
    
    public List<TransparencyUI> onScreenPowerupIcon = new List<TransparencyUI>();
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        EventManager.onRoundStart += roundStart;
        EventManager.onRoundEnd += roundEnd;
        for (int i = 0; i < onScreenPowerupIcon.Count; i++)
            hideIOnScreenPowerUpIcon(i);
        clearText();
    }

    public TransparencyUI getOnScreenPowerUpIcon(int index)
    {
        return onScreenPowerupIcon[index];
    }

    public void hideIOnScreenPowerUpIcon(int index)
    {
        //onScreenPowerupIcon.enabled = false;
        onScreenPowerupIcon[index].snapTransparency(0);
    }

    public void showIOnScreenPowerUpIcon(int index)
    {
        Debug.Log("Current index: " + index);
        if(GameManager.instance.powerupManager[index].getCurrentPowerUp() == powerUpList.None)
        {
            hideIOnScreenPowerUpIcon(index);
            return;
        }
        onScreenPowerupIcon[index].snapTransparency(1);
        //onScreenPowerupIcon.enabled = true;

    }

    public void roundStart(object sender, RoundArgs e)
    {
        if(e.getRound(0) == roundType.NONE)
        {
            for (int i = 0; i < onScreenPowerupIcon.Count; i++)
                hideIOnScreenPowerUpIcon(i);
        }
        
    }

    public void roundEnd(object sender, RoundArgs e)
    {

        Debug.Log("Count: " + onScreenPowerupIcon.Count);
        if (e.getRound(0) == roundType.NONE)
        {
            for (int i = 0; i < onScreenPowerupIcon.Count; i++)
            {
                //if (!onScreenPowerupIcon[i].gameObject.activeSelf) return;
                
                //showIOnScreenPowerUpIcon(i);
            }
        }
      
    }

    public void setPowerUpIconImage(Texture image, float lengthOfPowerUp, int index)
    {
        if(image == null || GameManager.instance.powerupManager[index].getCurrentPowerUp() == powerUpList.None)
        {
            hideIOnScreenPowerUpIcon(index);
            return;
        }
        //showIOnScreenPowerUpIcon();
        onScreenPowerupIcon[index].queueFadeTransparency(1f, 0.5f);
        onScreenPowerupIcon[index].setTransparencyImage(image);
    }

    public void setPowerUpIconImageByPowerUpType(powerUpList type, float length, int index)
    {
        setPowerUpIconImage(getPowerUpIconByType(type),length, index);
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
        //text.text = s;
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
