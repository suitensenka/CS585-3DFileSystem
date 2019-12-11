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

    public GameObject viewPanel, helixInstruction, wheelInstruction1, wheelInstruction2, backButton;


    public bool IsWorld = true;
    public bool IsWheel = false;
    public bool IsHelix = false;
    private float wheelY, defaultRadius, defaultDegree;


    public MyDataNode currentSelectedNode;
    public float delay = 0.5f;
    private float clicks = 0, prevClickTime = 0;
    private RaycastHit previousHit;
    private bool prevHit;
    private int ? prevHitID = null;

    public List<GameObject> Drives;
    public bool canClick = false;
    public Camera mainCamera;

    private Vector3 savedPosition;
    private Quaternion savedRotation;

    // Start is called before the first frame update
    void Start()
    {
        wheelY = transform.position.y - 5f;
        defaultRadius = radius;
        defaultDegree = degree;
        txtSelectedNode.text = "";
        txtHoveredOverNode.text = "";

        SavePosition();

        Drives = new List<GameObject>();

        float index = 0;
        foreach (var drive in DriveInfo.GetDrives())
        {
            //Debug.Log($"Drive: {drive.Name} Root: { drive.RootDirectory}");

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

            dn.UsedSpace = dn.Size - drive.TotalFreeSpace;
            dn.TFreeSpace = drive.TotalFreeSpace;
            dn.AFreeSpace = drive.AvailableFreeSpace;
            dn.driveType = drive.DriveType.ToString();
            dn.format = drive.DriveFormat;

            dn.ParentObject = InactiveFolder;
            dn.CurrentPosition = gObj.transform.position;

            var textName = Instantiate(TextMeshProPrefab, gObj.transform);
            textName.transform.localScale = new Vector3(1f, 1f, 1f);
            textName.GetComponent<TextMeshPro>().text = dn.Name;
            textName.transform.SetParent(gObj.transform);
            var height = gObj.transform.GetComponent<MeshRenderer>().bounds.size.y;
            textName.transform.position = new Vector3(gObj.transform.position.x, textName.transform.position.y - height / 1.5f, gObj.transform.position.z);
            textName.transform.localScale *= 0.2f;

            Drives.Add(gObj);
            index += 3f;
        }
        
        Vector3 endVector = new Vector3(transform.position.x, Drives[Drives.Count - 1].transform.position.y, transform.position.z);

        StartCoroutine(GetComponent<MyCameraControl>().ScanDrives(transform.position, endVector, 5.0f));
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
        if (Input.GetMouseButtonDown(0) && canClick)
        {
            Debug.Log("Click");
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
                int? hitID = null;
                if (hit)
                {
                    hitID = hitInfo.collider.gameObject.GetInstanceID();
                }
                 
                if (hit && (prevHitID == hitID))
                {
                    prevHitID = null; // Reset 
                    if (!InactiveFolder.activeSelf)
                        InactiveFolder.SetActive(false);

                    if (hitInfo.transform.GetComponent<MyDataNode>() != null)
                    {
                        IsWorld = false;
                        IsHelix = true;

                        ToggleViewPanel();
                        ToggleInstruction();
                        ToggleBack();
                        // if there is a hit, we want to get the DataNode component to extract the information
                        MyDataNode dn = hitInfo.transform.GetComponent<MyDataNode>();

                        //if it is not authorized to access. Prevent user from going further.
                        if (dn.isRestricted || dn.system)
                        {
                            canClick = false;
                            GetComponent<MyCameraControl>().canMove = false;
                            FileInfo fileInfo = GetComponent<FileInfo>();
                            fileInfo.RestrictedAccess.SetActive(true);
                        }

                        else
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
                            
                            dn.IsSelected = true;

                            transform.position = dn.gameObject.transform.position;

                            if (!dn.IsExpanded)
                                dn.ProcessNode(DoorPrefab, FilePrefab, TextMeshProPrefab);

                            if (dn.IsDrive)
                            {
                                HideAllDrives();
                            }

                            if (dn.IsFolder || dn.IsDrive)
                            {
                                dn.IsExpanded = true;
                                dn.gameObject.transform.SetParent(ActiveFolder.transform);
                            }

                            if (currentSelectedNode == null)
                            {
                                currentSelectedNode = dn;
                                DisableNode(currentSelectedNode.gameObject);
                                if (dn.IsExpanded)
                                {
                                    currentSelectedNode.transform.GetChild(0).gameObject.SetActive(true);
                                }

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

                            // if (IsHelix)
                            //     DisplayHelix();
                            // else
                            //     DisplayWheel();
                            //purpose of the WheelDisplay is to allow user to have an overall idea of the file size and its location.
                            //default view is always going to be helix
                            IsHelix = true;
                            DisplayHelix();
                        }
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
                    txtSelectedNode.text = $"{dn.Name}";
                    
                    prevHitID = hitInfo.collider.gameObject.GetInstanceID();

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
                else
                {
                    Debug.Log("No Hit");
                    prevHitID = null;
                    if(prevHit != false)
                    {
                        previousHit.transform.gameObject.GetComponent<MeshRenderer>().materials[1].SetFloat("_Outline", 0f);
                    }
                    txtSelectedNode.text = $"";
                }
                Debug.Log("Single Click");

            }
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                prevHitID = hitInfo.collider.gameObject.GetInstanceID();
                
            }
            else
            {
                prevHitID = null;
            }
            prevClickTime = Time.time;
            clicks++;
            Debug.Log("Click++");

        }

        if (Input.GetMouseButtonDown(1) && canClick)
        {
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit)
            {
                
                MyDataNode dn = hitInfo.transform.GetComponent<MyDataNode>();

                if (dn.IsFolder || dn.IsFile)
                {

                    canClick = false;
                    GetComponent<MyCameraControl>().canMove = false;

                    FileInfo fileInfo = GetComponent<FileInfo>();

                    fileInfo.InfoPanel.SetActive(true);

                    if (dn.IsFolder)
                        fileInfo.fileType.text = "Folder";
                    if (dn.IsDrive)
                        fileInfo.fileType.text = "Drive";
                    if (dn.IsFile)
                        fileInfo.fileType.text = "File";

                    fileInfo.fileName.text = dn.Name;
                    fileInfo.location.text = Path.GetDirectoryName(dn.FullName);


                    fileInfo.size.text = dn.Size.ToString() + " bytes";
                    if (dn.IsFolder)
                    {
                        fileInfo.size.text = "-";
                    }
                    
                    if (dn.isRestricted || dn.system)
                    {
                        fileInfo.size.text = "<color=red>Restricted Access.</color>";
                    }
                    fileInfo.folderCount.text = dn.FolderCount.ToString();
                    fileInfo.created.text = File.GetCreationTime(dn.FullName).ToString("g");
                    fileInfo.modified.text = File.GetLastWriteTime(dn.FullName).ToString("g");
                    fileInfo.accessed.text = File.GetLastAccessTime(dn.FullName).ToString("g");
                    fileInfo.attributes.text = File.GetAttributes(dn.FullName).ToString();
                }

                else if (dn.IsDrive)
                {
                    canClick = false;
                    GetComponent<MyCameraControl>().canMove = false;
                    FileInfo fileInfo = GetComponent<FileInfo>();

                    fileInfo.DrivePanel.SetActive(true);
                    fileInfo.driveName.text = dn.FullName;
                    fileInfo.driveType.text = dn.driveType;
                    fileInfo.format.text = dn.format;
                    fileInfo.dSize.text = dn.Size.ToString() + " bytes";
                    fileInfo.userfspace.text = dn.AFreeSpace.ToString() + " bytes";
                    fileInfo.fspace.text = dn.TFreeSpace.ToString() + " bytes";
                    fileInfo.usedspace.text = dn.UsedSpace.ToString() + " bytes";
                }
            }
        }
    }

    public void HideAllDrives()
    {
        foreach (GameObject child in Drives)
        {
            child.transform.SetParent(InactiveFolder.transform.GetChild(0));
        }
    }

    public void ShowAllDrives()
    {
        foreach (GameObject child in Drives)
        {
            child.transform.SetParent(ActiveFolder.transform);
            EnableNode(child.gameObject);
        }

    }

    public void ToggleInstruction()
    {
        if(!IsWorld)
        {
            helixInstruction.SetActive(true);
            wheelInstruction1.SetActive(true);
            wheelInstruction2.SetActive(true);
        }
        else
        {
            helixInstruction.SetActive(false);
            wheelInstruction1.SetActive(false);
            wheelInstruction2.SetActive(false);
        }
    }

    public void ToggleViewPanel()
    {
        if(!IsWorld)
        {
            viewPanel.SetActive(true);
        }
        else
            viewPanel.SetActive(false);
    }

    public void ToggleBack()
    {
        if(!IsWorld)
        {
            backButton.SetActive(true);
        }
        else
            backButton.SetActive(false);
    }


    public void DisableNode(GameObject obj)
    {
        obj.GetComponent<MeshRenderer>().enabled = false;
        try
        {
            obj.transform.GetChild(1).gameObject.SetActive(false);
        }
        catch { };
    }

    public void EnableNode(GameObject obj)
    {
        obj.GetComponent<MeshRenderer>().enabled = true;
        try
        {
            obj.transform.GetChild(1).gameObject.SetActive(true);
        }
        catch { };
    }


    public void GoBackUp()
    {
        if (currentSelectedNode == null)
            return;
        if (!currentSelectedNode.IsDrive)
        {
            IsWorld = false;

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


            IsWheel = false;
            IsHelix = true;
            DisplayHelix();
            txtSelectedNode.text = $"{currentSelectedNode.name}";
        }
        else if (currentSelectedNode.IsDrive)
        {
            IsWorld = true;
            currentSelectedNode.transform.GetChild(0).gameObject.SetActive(false);

            currentSelectedNode = null;

            ShowAllDrives();
            ToggleInstruction();
            ToggleViewPanel();
            ToggleBack();
            //restore state;
            transform.position = GetComponent<MyCameraControl>().defaultPosition;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            txtSelectedNode.text = $"";
        }
    }

    public void DisplayHelix()
    {
        int i = 0;

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
        SavePosition();
    }

    public void DisplayWheel()
    {
        int i = 0;
        float prev = 0f;

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
            child.transform.LookAt(transform);

        }
        radius = defaultRadius;
        degree = defaultDegree;
        SavePosition();
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

    private void SavePosition()
    {
        savedPosition = transform.position;
        savedRotation = transform.rotation;
    }

    public void ResetPosition()
    {
        transform.position = savedPosition;
        transform.rotation = savedRotation;
    }
}
