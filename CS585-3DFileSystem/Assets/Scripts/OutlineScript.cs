using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineScript : MonoBehaviour
{
    // Start is called before the first frame update

    Material m;
    void Start()
    {
        m = GetComponent<MeshRenderer>().materials[1];
    }

    void OnMouseOver()
    {
        //Debug.Log("Hovering");
        m.SetFloat("_Outline", 0.0007f);
    }

    void OnMouseExit()
    {
        m.SetFloat("_Outline", 0f);
    }

    
}
