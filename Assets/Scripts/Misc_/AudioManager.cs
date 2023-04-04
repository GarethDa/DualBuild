using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioSource audioSource2; //for external sounds (not music)
    public AudioClip music1; //Menu Music
    public AudioClip music2; //Lobby Music
    public AudioClip music3; //Dual Music
    public AudioClip audio1;

    public bool startDual = false;
    public bool startLobby = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource2 = GetComponent<AudioSource>();
        audioSource.volume = Volume.volumeSet; //Get volume set from main menu only
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (RoundManager.inPreview == true)
        {
            if (startDual == false)
            {
                playDualMusic();
            }
            startDual = true;
        }
        */
        if (RoundManager.inIntermission == true)
        {
            if (startLobby == false)
            {
                playLobbyMusic();
            }
            startLobby = true;
        }
        
    }

    public void playLobbyMusic()
    {
        audioSource.clip = music2;
        audioSource.Play();
    }

    public void playDualMusic()
    {
        audioSource.clip = music3;
        audioSource.Play();
    }

    public void playActionCue()
    {
        audioSource2.PlayOneShot(audio1, 0.75f);
    }
}
