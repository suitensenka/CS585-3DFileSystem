using UnityEngine;

public class MouseClick : MonoBehaviour
{
    public float delay = 0.5f;

    private float clicks = 0;
    private float prevClickTime = 0;

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Left Click");

            float deltaTime = Time.time - prevClickTime;
            if(deltaTime <= delay)
            {
                clicks = 0;
                Debug.Log("Double Click");   
            }
            else if(clicks == 1)
            {
                clicks = 0;
                Debug.Log("Single Click");
            }

            prevClickTime = Time.time;
            clicks++;
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Right Click");
        }
    }
}
