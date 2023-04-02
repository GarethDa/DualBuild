using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField] private Camera uiCam;
    public Vector3 offset;

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = transform.localPosition - ((Vector3)eventData.position - 0.5f * new Vector3(uiCam.pixelWidth, uiCam.pixelHeight, 0));
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.localPosition = ((Vector3)eventData.position - 0.5f * new Vector3(uiCam.pixelWidth, uiCam.pixelHeight, 0)) + offset;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
