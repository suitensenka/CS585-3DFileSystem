using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class MouseClick : MonoBehaviour, IPointerDownHandler
{
    float clicks = 0;
    float clickStart = 0;
    const float delay = 0.5f;
    
    public void OnPointerDown(PointerEventData data)
    {
        clicks++;
        if(clicks == 1)
        {
            clickStart = Time.time;
        }
        if(clicks > 1 && (Time.time - clickStart) <= delay)
        {
            clicks = 0;
            clickStart = 0;
            Debug.Log("Double click detected");
        } 
        else if(clicks > 2 || Time.time - clickStart > delay)
        {
            clicks = 0;
            clickStart = 0;
            Debug.Log("Reset click");
        }
    }
}
