using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Intro : MonoBehaviour
{
    public GameObject textPopup;

    public float introLength;
    public float textPopupTime;

    // Start is called before the first frame update
    void Start()
    {
        //these start at the same time
        StartCoroutine(skipPopup());
        StartCoroutine(introWait());
    }

    //Text that shows how to skip aft set time
    IEnumerator skipPopup()
    {
        yield return new WaitForSeconds(textPopupTime);

        textPopup.SetActive(true);
    }

    //How long until the intro cutscene is done, then send to main menu by default
    IEnumerator introWait()
    {
        yield return new WaitForSeconds(introLength);

        textPopup.SetActive(false);
        SceneManager.LoadScene("newMainMenu");
    }
}
