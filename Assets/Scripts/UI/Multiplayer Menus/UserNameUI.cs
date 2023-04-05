using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserNameUI : SwappableUI
{
    public TMP_InputField field;
    public TMP_InputField IPInput;
    public TMP_Text error;
    
    public void buttonClick()
    {
        if(field.text.Length == 0)
        {
            error.text = "Enter a valid username!";
            return;
        }
        if (field.text.Length >= 27)
        {
            error.text = "Username must be between 1-26 characters!";
            return;
        }
        string requiredCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
        string forbiddenCharacters = "`~!@#$%^&*()_+|-={}[];:<>?,./";
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
        string validCharactersIP = "0123456789.";
        if (IPInput.text.Length == 0)
        {
            error.text = "Please enter a valid IP!";
            return;
        }
        foreach (char c in IPInput.text)
        {
            if (!validCharactersIP.Contains(c))
            {
                error.text = "Please enter a valid IP!";
                return;
            }
        }



        NetworkManager.instance.serverIP = IPInput.text;

        setUserName();
        SwappableUIManager.instance.showUI(UIMenuType.HOME);
        
    }

    public void setUserName()
    {
        NetworkManager.instance.username = field.text;
    }
}
