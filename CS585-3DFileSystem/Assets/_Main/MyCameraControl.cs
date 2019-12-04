using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCameraControl : MonoBehaviour
{
    public float minX = -360.0f;
    public float maxX = 360.0f;

    public float minY = -45.0f;
    public float maxY = 45.0f;

    public float sensX = 100.0f;
    public float sensY = 100.0f;

    float rotationY = 0.0f;
    float rotationX = 0.0f;

    public float yMod;
    private FileSystem fsscript;

    // Start is called before the first frame update
    void Start()
    {
        //yMod = transform.gameObject.GetComponent<MyFileSystem>().heightModifier;
        fsscript = transform.gameObject.GetComponent<FileSystem>();
    }
    private float speed = 2.0f;
    void Update()
    {

        if (fsscript.IsHelix)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(Vector3.up * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(-Vector3.up * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(0.0f, -(1f + speed * Time.deltaTime), 0.0f, Space.World);
                transform.Translate(new Vector3(0.0f, yMod, 0.0f) * Time.deltaTime);

            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Rotate(0.0f, 1f + speed * Time.deltaTime, 0.0f, Space.World);
                transform.Translate(new Vector3(0.0f, -yMod, 0.0f) * Time.deltaTime);
            }
        }

        else if (fsscript.IsWheel)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(-Vector3.forward * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(0.0f, -(1f + speed * Time.deltaTime), 0.0f, Space.World);

            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(0.0f, 1f + speed * Time.deltaTime, 0.0f, Space.World);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(-Vector3.left * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(Vector3.left * speed * Time.deltaTime);
            }

        }
    }
}
