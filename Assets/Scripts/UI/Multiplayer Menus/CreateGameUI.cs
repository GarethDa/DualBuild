using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGameUI : SwappableUI
{
    public void buttonStartGame()
    {
        NetworkManager.instance.startGame();
        SwappableUIManager.instance.hideAll();
        
    }
    
}
