using GameAudioScriptingEssentials;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDestroy : MonoBehaviour
{
    private bool isCalled = false;

    public void setCalled(bool b)
    {
        isCalled = b;
    }

    // Update is called once per frame
    void Update()
    {

        if (isCalled == true)
        {
            Debug.Log("KABOOM");
            if (GetComponent<AudioSource>() == null)
            {
                Destroy(gameObject);
                return;
            }

            if (GetComponent<AudioClipRandomizer>().SFXPlayPosition > GetComponent<AudioClipRandomizer>().GetSFXLength())
            {
                isCalled = false;
                Destroy(gameObject);
            }
        }
    }
}
