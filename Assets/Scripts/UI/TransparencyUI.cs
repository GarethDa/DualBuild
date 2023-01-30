using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

struct transparencyManager
{
   public float wantedTransparency;
   public float startingTransparency;
   public float transparencyLength;
}

public class TransparencyUI : MonoBehaviour
{
    [SerializeField] RawImage transparencyImage;
    float currentTransparency = 1f;
    float currentWantedTransparency = 0f;
    float currentStartingTransparency = 0f;
    Queue<transparencyManager> queuedTransparencies = new Queue<transparencyManager>();

    float currentTransparencyTime = 0f;
    
    int currentIndex = 0;
    //public bool stipple = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setTransparencyImage(Texture image)
    {
        transparencyImage.texture = image;
    }

    public RawImage getTransparencyImage()
    {
        return transparencyImage;
    }

    

    public void queueFadeTransparency(float wanted, float secondsLong, float starting = -1f)
    {
        //currentTransparencyTime = 0f;
        //Debug.Log("QUEUED WANTED" + wanted.ToString() + " LONG " + secondsLong.ToString()); ;
        transparencyManager manager = new transparencyManager();
        if (starting >= 0)
        {
            manager.startingTransparency = starting;

            setTransparency(starting);
        }
        

        manager.wantedTransparency = wanted;
        manager.transparencyLength = secondsLong;
        queuedTransparencies.Enqueue(manager);
        if(queuedTransparencies.Count == 1)
        {
            currentWantedTransparency = wanted;
            currentStartingTransparency = starting;
            if(starting < 0)
            {
                currentStartingTransparency = currentTransparency;
            }
        }
       
    }

    public void snapTransparency(float wanted)
    {
        Debug.Log("SNAPPED WANTED" + wanted.ToString()); ;
        currentTransparency = wanted;
        setTransparency(currentTransparency);
    }



    public float getWantedTransparency()
    {
        if(queuedTransparencies.Count == 0)
        {
            return -1;
        }
        return queuedTransparencies.Peek().wantedTransparency;
    }

    public float getCurrentTransparency()
    {
        return currentTransparency;
    }

    // Update is called once per frame
    void Update()
    {
        if(queuedTransparencies.Count == 0)
        {
            return;
        }
        if(queuedTransparencies.Peek().wantedTransparency >= 0)
        {
            calculateTransparency();
        }
        
    }

    void calculateTransparency()
    {
        
        currentTransparencyTime += Time.deltaTime ;

        if((currentTransparencyTime) >= queuedTransparencies.Peek().transparencyLength)
        {
            //Debug.Log(currentTransparencyTime.ToString() + " " + queuedTransparencies.Peek().transparencyLength.ToString());

            currentTransparency = currentWantedTransparency;
            
            setTransparency(currentTransparency);
            queuedTransparencies.Dequeue();
            currentTransparencyTime = 0f;

            if (queuedTransparencies.Count == 0)
            {
                return;
            }
            currentStartingTransparency = currentTransparency;
            if(queuedTransparencies.Peek().startingTransparency == -1)
            {
                currentStartingTransparency = queuedTransparencies.Peek().startingTransparency;
            }
            currentWantedTransparency = queuedTransparencies.Peek().wantedTransparency;
            
           // Debug.Log("DONE! NEXT");
            return;
        }
        //Debug.Log(currentTransparencyTime.ToString() + " " + queuedTransparencies.Peek().transparencyLength.ToString());
        
        currentTransparency = Mathf.Lerp(currentStartingTransparency, currentWantedTransparency, (currentTransparencyTime / queuedTransparencies.Peek().transparencyLength));
        setTransparency(currentTransparency);
        
    }

    void setTransparency(float transparency)
    {
        transparencyImage.color = new Color(1, 1, 1, transparency);
    }
}
