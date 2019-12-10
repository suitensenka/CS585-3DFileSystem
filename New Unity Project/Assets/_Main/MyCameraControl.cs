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


    public float yMod;
    private FileSystem fsscript;

    public bool canMove = false;
    public Vector3 defaultPosition;

    // Start is called before the first frame update
    private float speed = 2.0f;
    void Start()
    {
        //yMod = transform.gameObject.GetComponent<MyFileSystem>().heightModifier;
        fsscript = transform.gameObject.GetComponent<FileSystem>();
        defaultPosition = transform.position;
    }
    void Update()
    {
        if (fsscript.IsWorld && canMove)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(Vector3.up * (speed + 2f) * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(-Vector3.up * (speed+2f) * Time.deltaTime);
            }
        }


        if (fsscript.IsHelix && canMove)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(Vector3.up * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(-Vector3.up * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Rotate(0.0f, -(1f + speed * Time.deltaTime), 0.0f, Space.World);
                transform.Translate(new Vector3(0.0f, yMod, 0.0f) * Time.deltaTime);

            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(0.0f, 1f + speed * Time.deltaTime, 0.0f, Space.World);
                transform.Translate(new Vector3(0.0f, -yMod, 0.0f) * Time.deltaTime);
            }
        }

        else if (fsscript.IsWheel && canMove)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                transform.Translate(Vector3.forward * (speed + 0.5f) * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.X))
            {
                transform.Translate(-Vector3.forward * (speed + 0.5f) * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(0.0f, -(1f + speed * Time.deltaTime), 0.0f, Space.World);
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(0.0f, 1f + speed * Time.deltaTime, 0.0f, Space.World);
            }

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
                transform.Translate(-Vector3.left * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(Vector3.left * speed * Time.deltaTime);
            }

        }
    }


    public IEnumerator ScanDrives(Vector3 start, Vector3 end, float speed)
    {
        while (transform.position != end)
        {
            transform.position = Vector3.MoveTowards(transform.position, end, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        while (transform.position != start)
        {
            transform.position = Vector3.MoveTowards(transform.position, start, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        canMove = true;
        GetComponent<FileSystem>().canClick = true;
    }
}
