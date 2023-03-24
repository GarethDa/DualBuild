using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserNameUI : SwappableUI
{
    public TMP_InputField field;
    public TMP_Text error;
    
    public void buttonClick()
    {
        if(field.text.Length == 0)
        {
            error.text = "Enter a valid username!";
            return;
        }
        string requiredCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_";
        string forbiddenCharacters = "`~!@#$%^&*()_+|-={}[];:<>?,.";
        bool hasRequiredCharacter = false;
        bool hasForbiddenCharacter = false;
        foreach(char c in field.text)
        {
            if (requiredCharacters.Contains(c.ToString()))
            {
                hasRequiredCharacter = true;
                
            }
            if (forbiddenCharacters.Contains(c.ToString()))
            {
                hasForbiddenCharacter = true;
            }
        }
        if (!hasRequiredCharacter)
        {
            error.text = "Enter a valid username!";
            return;
        }
        if (hasForbiddenCharacter)
        {
            error.text = "Please use alphanumeric characters for your username!";
            return;
        }
        setUserName();
        SwappableUIManager.instance.showUI(UIMenuType.HOME);
        
    }

    public void setUserName()
    {
        NetworkManager.instance.username = field.text;
    }
}
