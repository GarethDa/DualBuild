using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinGameUI : SwappableUI
{
    bool isShown = false;

    public override void show()
    {
        isShown = true;
        base.show();
    }

    public override void hide()
    {
        if (isShown)
        {
            SwappableUIManager.instance.hideAll();
        }
        base.hide();
    }
}
