using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;


public class MyFileSystem : MonoBehaviour
{
    public Text txtSelectedNode;
    public Text txtHoveredOverNode;

    public GameObject ActiveFolder;
    public GameObject InactiveFolder;

    //PREFABS
    public GameObject DoorPrefab, TextMeshProPrefab;
    public float degree, degreeModifier, radius, heightModifier; //control how large the helix is.


    public bool IsWheel = false;
    public bool IsHelix = true;
    private float wheelY, defaultRadius;

    public DataNode currentSelectedNode;

    // Start is called before the first frame update
    void Start()
    {
        wheelY = transform.position.y - 5f;
        defaultRadius = radius;
        txtSelectedNode.text = "";
        txtHoveredOverNode.text = "";

        float index = 0;
        foreach (var drive in DriveInfo.GetDrives())
        {
            Debug.Log($"Drive: {drive.Name} Root: { drive.RootDirectory}");

            // Create a primitive type cube game object
            var gObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // Calculate the position of the game object in the world space
            int x = 0;
            float y = index + 1f;
            int z = 0;

            // Position the game object in world space
            gObj.transform.position = new Vector3(x, y, z);
            gObj.name = drive.Name;

            // Add DataNode component and update the attributes for later usage
            gObj.AddComponent<DataNode>();
            DataNode dn = gObj.GetComponent<DataNode>();
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
        #region HANDLE MOUSE INTERACTION
        // Create a raycase from the screen-space into World Space, store the data in hitInfo Object
        bool Hoverhit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
        if (Hoverhit)
        {
            if (hitInfo.transform.GetComponent<DataNode>() != null)
            {
                // if there is a hit, we want to get the DataNode component to extract the information
                DataNode dn = hitInfo.transform.GetComponent<DataNode>();
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
            if(EventSystem.current.IsPointerOverGameObject())
            {
                return; //return if we are clicking on UI.
            }

            // Create a raycase from the screen-space into World Space, store the data in hitInfo Object
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit)
            {
                if (!InactiveFolder.activeSelf)
                    InactiveFolder.SetActive(false);
                if (hitInfo.transform.GetComponent<DataNode>() != null)
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
                    }                    // if there is a hit, we want to get the DataNode component to extract the information
                    DataNode dn = hitInfo.transform.GetComponent<DataNode>();

                    if (dn.IsFolder && !dn.IsExpanded)
                    {
                        DirectoryInfo diTop = new DirectoryInfo(dn.FullName);
                        int samples = diTop.GetDirectories("*").Length;
                        dn.gameObject.transform.Translate(Vector3.forward * -(samples % 2) * 1.5f, Space.Self);

                        //dn.NewPosition = (Vector3.forward * (samples % 2));
                        //dn.Move = true;

                        // update line renderer component
                        //hitInfo.transform.GetComponent<LineRenderer>().SetPosition(1, dn.gameObject.transform.position);


                        diTop = null;

                    }

                    txtSelectedNode.text = $"Selected Node: {dn.FullName} Size Is: {dn.Size}";
                    dn.IsSelected = true;

                    if (!dn.IsExpanded)
                        dn.ProcessNode(DoorPrefab, TextMeshProPrefab, degree, degreeModifier, radius, heightModifier, IsHelix, IsWheel);
                    if (dn.IsFolder | dn.IsDrive)
                    {
                        dn.IsExpanded = true;
                        dn.gameObject.transform.SetParent(ActiveFolder.transform);
                    }

                    if (currentSelectedNode == null)
                    {
                        currentSelectedNode = dn;
                    }
                    else
                    {
                        currentSelectedNode.transform.position = currentSelectedNode.CurrentPosition;
                        currentSelectedNode.transform.GetChild(0).gameObject.SetActive(true);
                        if (!currentSelectedNode.FullName.Equals(dn.FullName))
                            currentSelectedNode.transform.SetParent(currentSelectedNode.ParentObject.transform.GetChild(0));
                        currentSelectedNode.IsSelected = false;
                        currentSelectedNode = dn;
                        if (dn.IsExpanded)
                        {
                            currentSelectedNode.transform.GetChild(0).gameObject.SetActive(true);
                        }
                    }
                    

                }
            }
        }
    }

    public void GoBackUp()
    {
        if (!currentSelectedNode.IsDrive)
        {

            currentSelectedNode.transform.GetChild(0).gameObject.SetActive(false);
            currentSelectedNode.transform.position = currentSelectedNode.CurrentPosition;
            currentSelectedNode.transform.SetParent(currentSelectedNode.ParentObject.transform.GetChild(0));
            currentSelectedNode.IsSelected = false;

            //move parent node back to visible node
            currentSelectedNode.ParentObject.transform.SetParent(ActiveFolder.transform);
            currentSelectedNode.ParentObject.transform.GetChild(0).gameObject.SetActive(true);
            currentSelectedNode.ParentObject.transform.position = currentSelectedNode.ParentObject.transform.GetComponent<DataNode>().CurrentPosition;

            currentSelectedNode = currentSelectedNode.ParentObject.transform.GetComponent<DataNode>();

            transform.position = currentSelectedNode.transform.position;

            if(IsHelix)
                SwitchHelix();
            else
                SwitchWheel();
        }
    }


    public void SwitchHelix()
    {
        if (!IsHelix)
        {
            IsHelix = true;
            IsWheel = false;

            transform.position = currentSelectedNode.transform.position; //restore the position.
            //get first children object in hideexpanded if it is not an empty folder.
            try{
                Transform childObj = currentSelectedNode.transform.GetChild(0).GetChild(0); //Try to get the first child. This will throw an exception if there isn't one. i.e. empty.
                Vector3 newLookAt = new Vector3(childObj.position.x, transform.position.y, transform.position.z);
                transform.LookAt(newLookAt);
            }
            catch
            {
                transform.rotation = Quaternion.Euler(0f,0f,0f);
            }


            //Vector3 newLookAt = new Vector3(currentSelectedNode.transform.position.x, transform.postion.y, transform.position.z);
            //transform.LookAt(newLookAt);

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
                }
                radius = defaultRadius;
            }
            catch
            {

            }
        }

    }

    public void SwitchWheel()
    {
        if (!IsWheel)
        {
            IsHelix = false;
            IsWheel = true;
            transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            transform.position = new Vector3(transform.position.x, wheelY, transform.position.z);

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
            }
            catch
            {

            }
        }


    }
}
