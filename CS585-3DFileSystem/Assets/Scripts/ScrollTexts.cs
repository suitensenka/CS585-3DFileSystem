using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
public class ScrollTexts : MonoBehaviour
{
    public TextMeshPro tm;
    public RectTransform rect;
    public float speed;
    public int secondsToWait;
    private bool hasTextChanged = false;
    private float width;
    // Start is called before the first frame update

    void Awake()
    {
        tm = transform.GetChild(0).GetComponent<TextMeshPro>(); //Getting the child of the objects
        rect = transform.GetChild(0).GetComponent<RectTransform>();
    }
    void Start()
    {
        StartCoroutine(Scrolls());
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTextChanged)
        {
            width = tm.preferredWidth;
            hasTextChanged = false;
            StopCoroutine(Scrolls());
            StartCoroutine(Scrolls());

        }
    }

    void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        StartCoroutine(Scrolls());
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
    }

    void ON_TEXT_CHANGED(object obj)
    {
        if (obj == tm)
        {
            hasTextChanged = true;
        }
    }

    IEnumerator Scrolls()
    {
        width = tm.preferredWidth;
        Vector3 startPos = rect.position;

        float scrollPos = 0f;

        while (true)
        {

            rect.position = new Vector3(-scrollPos, startPos.y, startPos.z);
            if(scrollPos % width <= width && scrollPos % width > width - 1)
            {
                //Debug.Log("HI");
                scrollPos += speed * Time.deltaTime;
                rect.position = startPos;
                //scrollPos = 0f;
                
                yield return new WaitForSeconds(secondsToWait);
                
            }
            scrollPos += speed * Time.deltaTime;
//            Debug.Log(width);
            yield return null;
        }
    }

}
