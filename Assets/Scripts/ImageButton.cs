using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class ImageButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public UnityEvent onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
       
        setChildImageColour(new Color(0.5f, 0.5f, 0.5f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
        setChildImageColour(Color.white);
    }

    public void setChildImageColour(Color c)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            RawImage image = transform.GetChild(i).GetComponent<RawImage>();
            if (image != null)
            {
                image.color = c;
            }
           
        }
    }
}
