using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DestructorBehaviour : MonoBehaviour
{
    [SerializeField] Transform gameManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyAll()
    {
        for (int i = 0; i < gameManager.childCount; i++)
        {
            Destroy(gameManager.GetChild(i).gameObject);
        }

        string newScene = RoundManager.instance.gameEndSceneName;
        SceneManager.LoadScene(newScene, LoadSceneMode.Single);

        //SceneManager.SetActiveScene(SceneManager.GetSceneByName(newScene));
        EventManager.unsubscribeAll();
        //SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("playgroundv4_single"));
    }
}
