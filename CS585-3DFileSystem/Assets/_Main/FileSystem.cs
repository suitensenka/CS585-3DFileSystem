using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

public class FileSystem : MonoBehaviour
{
    public Text txtSelectedNode;
    public Text txtHoveredOverNode;

    public GameObject ActiveFolder;
    public GameObject InactiveFolder;

    //PREFABS
    public GameObject DoorPrefab, FilePrefab, DrivePrefab, TextMeshProPrefab;
    public float degree = 0f, degreeModifier = 0.2f, radius = 1.5f, heightModifier = 0.05f;


    public bool IsWheel = false;
    public bool IsHelix = true;
    private float wheelY, defaultRadius, defaultDegree;

    public MyDataNode currentSelectedNode;
    public float delay = 0.5f;
    private float clicks = 0, prevClickTime = 0;
    private RaycastHit previousHit;
    private bool prevHit;

    // Start is called before the first frame update
    void Start()
    {
        wheelY = transform.position.y - 5f;
        defaultRadius = radius;
        defaultDegree = degree;
        txtSelectedNode.text = "";
        txtHoveredOverNode.text = "";

        float index = 0;
        foreach (var drive in DriveInfo.GetDrives())
        {
            Debug.Log($"Drive: {drive.Name} Root: { drive.RootDirectory}");

            // Create a primitive type cube game object
            var gObj = Instantiate(DrivePrefab);

            // Calculate the position of the game object in the world space
            int x = 0;
            float y = index + 1f;
            int z = 0;

            // Position the game object in world space
            gObj.transform.position = new Vector3(x, y, z);
            gObj.name = drive.Name;

            // Add DataNode component and update the attributes for later usage
            gObj.AddComponent<MyDataNode>();
            MyDataNode dn = gObj.GetComponent<MyDataNode>();
            dn.Name = drive.Name;
            dn.Size = drive.TotalSize;
            dn.FullName = drive.RootDirectory.FullName;
            dn.IsDrive = true;

            dn.ParentObject = InactiveFolder;
            dn.CurrentPosition = gObj.transform.position;

            var textName = Instantiate(TextMeshProPrefab, gObj.transform);
            textName.transform.localScale = new Vector3(1f, 1f, 1f);
            textName.GetComponent<TextMeshPro>().text = dn.Name;
            textName.transform.SetParent(gObj.transform);

            index += 3f;
        }
    }

    RaycastHit hitInfo = new RaycastHit();

    void Update()
    {
        #region HANDLE MOUSE INTERACTION 2
        // Create a raycase from the screen-space into World Space, store the data in hitInfo Object
        bool Hoverhit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
        if (Hoverhit)
        {
            if (hitInfo.transform.GetComponent<MyDataNode>() != null)
            {
                // if there is a hit, we want to get the DataNode component to extract the information
                MyDataNode dn = hitInfo.transform.GetComponent<MyDataNode>();
                txtHoveredOverNode.text = $"{dn.FullName}";
            }
        }
        else
        {
            txtHoveredOverNode.text = $"";
        }
        #endregion


        // Check to see if the Left Mouse Button was clicked
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; //return if we are clicking on UI.
            }

            float deltaTime = Time.time - prevClickTime;

            if (deltaTime <= delay)
            {
                clicks = 0;
                Debug.Log("Double Clicked");

                // Create a raycase from the screen-space into World Space, store the data in hitInfo Object
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                if (hit)
                {
                    if (!InactiveFolder.activeSelf)
                        InactiveFolder.SetActive(false);


                    if (hitInfo.transform.GetComponent<MyDataNode>() != null)
                    {
                        transform.position = hitInfo.transform.position;
                        if (IsWheel)
                        {
                            transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
                            transform.position = new Vector3(transform.position.x, wheelY, transform.position.z);
                        }
                        else if (IsHelix)
                        {
                            transform.rotation = Quaternion.identity;
                            transform.position = hitInfo.transform.position;
                        }


                        // if there is a hit, we want to get the DataNode component to extract the information
                        MyDataNode dn = hitInfo.transform.GetComponent<MyDataNode>();

                        txtSelectedNode.text = $"Selected Node: {dn.FullName} Size Is: {dn.Size}";
                        dn.IsSelected = true;

                        transform.position = dn.gameObject.transform.position;

                        if (!dn.IsExpanded)
                            dn.ProcessNode(DoorPrefab, FilePrefab, TextMeshProPrefab);

                        if (dn.IsFolder || dn.IsDrive)
                        {
                            dn.IsExpanded = true;
                            dn.gameObject.transform.SetParent(ActiveFolder.transform);
                        }

                        if (currentSelectedNode == null)
                        {
                            currentSelectedNode = dn;
                            DisableNode(currentSelectedNode.gameObject);

                        }
                        else
                        {
                            //currentSelectedNode.transform.position = currentSelectedNode.CurrentPosition;
                            //currentSelectedNode.transform.GetChild(0).gameObject.SetActive(true);

                            currentSelectedNode.transform.SetParent(InactiveFolder.transform);

                            if (!currentSelectedNode.FullName.Equals(dn.FullName))
                                currentSelectedNode.transform.SetParent(currentSelectedNode.ParentObject.transform.GetChild(0));

                            currentSelectedNode.IsSelected = false;

                            EnableNode(currentSelectedNode.gameObject); //reanable the previous one

                            currentSelectedNode = dn;
                            DisableNode(currentSelectedNode.gameObject); //disable the current one

                            if (dn.IsExpanded)
                            {
                                currentSelectedNode.transform.GetChild(0).gameObject.SetActive(true);
                            }
                        }

                        if (IsHelix)
                            DisplayHelix();
                        else
                            DisplayWheel();
                    }
                }
            }//END DOUBLECLICK

            else if (clicks == 1)
            {
                clicks = 0;
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                if (hit)
                {
                    Debug.Log(hitInfo.transform.name);
                    MyDataNode dn = hitInfo.transform.GetComponent<MyDataNode>();

                    if (prevHit == false)
                    {
                        previousHit = hitInfo;
                        prevHit = hit; //use to check that its not null
                    }
                    else
                    {
                        previousHit.transform.gameObject.GetComponent<MeshRenderer>().materials[1].SetFloat("_Outline", 0f);
                        previousHit = hitInfo;
                    }

                    if (dn.IsDrive || dn.IsFile)
                    {
                        hitInfo.transform.gameObject.GetComponent<MeshRenderer>().materials[1].SetFloat("_Outline", 0.03f);
                    }
                    else
                    {
                        hitInfo.transform.gameObject.GetComponent<MeshRenderer>().materials[1].SetFloat("_Outline", 0.0007f);
                    }
                }
                Debug.Log("Single Click");

            }
            prevClickTime = Time.time;
            clicks++;
        }
    }


    public void DisableNode(GameObject obj)
    {
        obj.GetComponent<MeshRenderer>().enabled = false;
        obj.transform.GetChild(1).gameObject.SetActive(false);
    }

    public void EnableNode(GameObject obj)
    {
        obj.GetComponent<MeshRenderer>().enabled = true;
        obj.transform.GetChild(1).gameObject.SetActive(true);
    }


    public void GoBackUp()
    {
        if (!currentSelectedNode.IsDrive)
        {

            currentSelectedNode.transform.GetChild(0).gameObject.SetActive(false);
            //currentSelectedNode.transform.position = currentSelectedNode.CurrentPosition;

            currentSelectedNode.transform.SetParent(currentSelectedNode.ParentObject.transform.GetChild(0));
            currentSelectedNode.IsSelected = false;

            //move parent node back to visible node
            currentSelectedNode.ParentObject.transform.SetParent(ActiveFolder.transform);
            currentSelectedNode.ParentObject.transform.GetChild(0).gameObject.SetActive(true);
            //currentSelectedNode.ParentObject.transform.position = currentSelectedNode.ParentObject.transform.GetComponent<MyDataNode>().CurrentPosition;

            EnableNode(currentSelectedNode.gameObject);

            currentSelectedNode = currentSelectedNode.ParentObject.transform.GetComponent<MyDataNode>();
            DisableNode(currentSelectedNode.gameObject);

            transform.position = currentSelectedNode.transform.position;

            if (IsHelix)
                DisplayHelix();
            else
                DisplayWheel();
        }
    }

    public void DisplayHelix()
    {
        int i = 0;
        try
        {
            foreach (Transform child in currentSelectedNode.transform.GetChild(0))
            {
                i++;
                float x = radius * Mathf.Cos(degree);
                float z = radius * Mathf.Sin(degree);
                float y = currentSelectedNode.transform.position.y;

                degree = degree + degreeModifier;
                child.position = new Vector3(x + currentSelectedNode.transform.position.x, y + heightModifier * i, z + currentSelectedNode.transform.position.z);
                Vector3 newRotate = new Vector3(transform.position.x, child.transform.position.y, transform.position.z);
                child.transform.LookAt(newRotate);
                transform.position = new Vector3(transform.position.x, child.position.y, transform.position.z);
                transform.LookAt(child);
            }
            radius = defaultRadius;
            degree = defaultDegree;

            if (i == 0)//Meaning it is empty. Reset the camera's rotation.
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }


        }
        catch
        {

        }
    }

    public void DisplayWheel()
    {
        int i = 0;
        float prev = 0f;
        try
        {
            foreach (Transform child in currentSelectedNode.transform.GetChild(0))
            {
                i++;
                float temp = (float)degree / (float)(2f * Math.PI);
                if (Mathf.Floor(temp) > prev)
                {
                    prev = Mathf.Floor(temp);
                    radius = radius + Mathf.Floor(temp);
                }
                float x = radius * Mathf.Cos(degree);
                float z = radius * Mathf.Sin(degree);
                float y = currentSelectedNode.transform.position.y;

                degree = degree + degreeModifier;
                child.position = new Vector3(x + currentSelectedNode.transform.position.x, y, z + currentSelectedNode.transform.position.z);
                //Vector3 newRotate = new Vector3(transform.position.x, child.transform.position.y, transform.position.z);
                child.transform.LookAt(transform);
            }
            radius = defaultRadius;
            degree = defaultDegree;
        }
        catch
        {

        }

    }

    public void SwitchHelix()
    {
        if (!IsHelix)
        {
            IsHelix = true;
            IsWheel = false;

            DisplayHelix();
        }

    }

    public void SwitchWheel()
    {
        if (!IsWheel && currentSelectedNode != null)
        {
            IsHelix = false;
            IsWheel = true;
            transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            transform.position = new Vector3(transform.position.x, wheelY, transform.position.z);

            DisplayWheel();

        }


    }
}
