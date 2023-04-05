using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class SkipIntro : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;

    public GameObject skipMeterObject;
    public Slider skipMeter;
    public float maxMeter = 1.5f;
    //public float minMeter = 0;
    //public float currentSkipMeter = 0.0f;

    public VideoPlayer videoPlayer;
    public float skipTime = 1.5f;

    private bool isSkipping = false;
    private float skipTimer = 0.0f;
    void Start()
    {
        playerInput.actions.FindActionMap("Intro").Enable();
        //currentSkipMeter = minMeter;
        skipMeter.maxValue = maxMeter;
        //skipMeter.value = minMeter;
    }

    // Update is called once per frame
    void Update()
    { 
        if (isSkipping)
        {
            skipMeterObject.SetActive(true);
            skipTimer += Time.deltaTime;
            skipMeter.value = skipTimer;

            if (skipTimer >= skipTime)
            {
                //Skip
                videoPlayer.Stop();
                videoPlayer.targetTexture.Release();
                SkipCutscene();
            }
        }
        else
        {
            skipMeterObject.SetActive(false);
            skipMeter.value = 0.0f;
        }
    }

    public void OnKeyPressed(InputAction.CallbackContext cntxt)
    {
        if (cntxt.ReadValue<float>() > 0)
        {
            //skipping timer start
            isSkipping = true;
        }
        else
        {
            //stop skipping
            isSkipping = false;
            skipTimer = 0.0f;
        }
    }

    public void SkipCutscene()
    {
        GetComponent<Intro>().textPopup.SetActive(false);
        skipMeterObject.SetActive(false);
        playerInput.actions.FindActionMap("Intro").Disable();
        Debug.Log("CUTSCENE SKIPPED");
        SceneManager.LoadScene("newMainMenu");
    }
}
