using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetStuff : MonoBehaviour
{
    [SerializeField] GameObject objectsToReset;

    Vector3 originalPos;

    GameObject objectCopy;
    GameObject currentStuff;

    // Start is called before the first frame update
    void Start()
    {
        originalPos = objectsToReset.transform.position;

        objectCopy = Instantiate(objectsToReset, originalPos, Quaternion.identity);

        objectCopy.SetActive(false);

        EventManager.onRoundStart += ResetObjects;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ResetObjects(object sender, RoundArgs e)
    {
        if (currentStuff != null)
            Destroy(currentStuff);

        else
            Destroy(objectsToReset);

        currentStuff = Instantiate(objectCopy, gameObject.transform, true);
        currentStuff.SetActive(true);
    }
}
