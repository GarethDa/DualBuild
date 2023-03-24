using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomKeyShower : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.onGetRoomKey += showKey;
        GetComponent<TMP_Text>().text = "Fetching room key...";
    }

    public void showKey(object sender, StringArgs e)
    {
        GetComponent<TMP_Text>().text = "Room Code: " + e.str;
    }
}
