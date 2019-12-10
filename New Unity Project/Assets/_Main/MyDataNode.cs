using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

public class MyDataNode : MonoBehaviour
{
    public string Name;
    public string FullName;
    public long Size;
    public long UsedSpace, TFreeSpace, AFreeSpace;
    public string format, driveType;
    public int FolderCount;
    public bool IsFolder = false;
    public bool IsDrive = false;

    public bool IsFile = false;
    public bool hidden = false, system = false, readOnly = false, isRestricted = false;

    public bool IsSelected = false;
    public bool IsExpanded = false;

    public bool Move = false;
    public Vector3 NewPosition;

    public Vector3 CurrentPosition; //Store the current position to replace it into the view later
    public GameObject ParentObject; //cache the parent object so we can move it back


    //drive.AvailableFreeSpace; 
    //drive.TotalFreeSpace;

    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    Transform p1;
    Transform p2;
    GameObject cGObj;

    public GameObject HideExpanded;
    public int lengthOfLineRenderer = 2;

    Transform parentNode;
    private FileSystem fsscript;

    public void ProcessNode(GameObject DoorPrefab, GameObject FilePrefab, GameObject TextMeshProPrefab)
    {
        //fsscript = transform.gameObject.GetComponent<MyFileSystem>();
        if (IsFolder || IsDrive)
        {
            // let's expand ...
            // Set a variable to the My Documents path.
            string docPath = FullName;

            DirectoryInfo diTop = new DirectoryInfo(docPath);

            int i = 0;

            try
            {
                //This is here to easily hide object that are not in the current directory.
                if (IsExpanded)
                {
                    HideExpanded = ParentObject.transform.GetChild(0).gameObject;
                }
                else
                {
                    HideExpanded = new GameObject("HideExpanded");
                    HideExpanded.transform.SetParent(transform);
                    HideExpanded.transform.SetSiblingIndex(0); //ensure that it is always at position 0
                }
                int samples = diTop.GetDirectories("*").Length;
                float rnd = 1;
                bool randomize = true;

                if (randomize)
                    rnd = UnityEngine.Random.value * samples;
                float offset = 2.0f / samples;
                float increment = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));

                foreach (var fi in diTop.EnumerateFiles())
                {
                    try
                    {

                        //var gObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        var gObj = Instantiate(FilePrefab);

                        gObj.transform.localScale *= 0.1f;

                        gObj.transform.SetParent(HideExpanded.transform);
                        gObj.name = fi.FullName;

                        gObj.AddComponent<MyDataNode>();
                        MyDataNode dn = gObj.GetComponent<MyDataNode>();
                        dn.Name = fi.Name;
                        dn.Size = fi.Length;
                        dn.FolderCount = 0;
                        dn.FullName = fi.FullName;
                        dn.IsFolder = false;
                        dn.IsFile = true;

                        ///HANDLING FILES PERMISSION
                        FileAttributes fileAttr = fi.Attributes;
                        dn.hidden = ((fileAttr & FileAttributes.Hidden) == FileAttributes.Hidden) ? true : false;
                        dn.system = ((fileAttr & FileAttributes.System) == FileAttributes.System) ? true : false;
                        dn.readOnly = ((fileAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? true : false;

                        if (dn.hidden)
                        {
                            gObj.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
                        }

                        if (dn.readOnly)
                        {
                            gObj.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.green);
                        }
                        if (dn.system)
                        {
                            gObj.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
                        }

                        //Storing information later to restore their original position in the helix
                        dn.ParentObject = transform.gameObject;
                        dn.CurrentPosition = gObj.transform.position;

                        var textName = Instantiate(TextMeshProPrefab, gObj.transform);
                        textName.GetComponent<TextMeshPro>().text = dn.Name;
                        textName.transform.SetParent(gObj.transform);
                        textName.transform.localScale *= 0.2f;
                        var height = gObj.transform.GetComponent<MeshRenderer>().bounds.size.y;
                        textName.transform.position = new Vector3(gObj.transform.position.x, textName.transform.position.y - height, gObj.transform.position.z);

                    }
                    catch (UnauthorizedAccessException unAuthTop)
                    {
                        Debug.LogWarning($"{unAuthTop.Message}");
                    }
                    i++;
                }

                //i = 0;
                foreach (var di in diTop.EnumerateDirectories("*"))
                {
                    var gObj = Instantiate(DoorPrefab);
                    try
                    {
                        gObj.transform.SetParent(HideExpanded.transform);
                        parentNode = transform;

                        float diScale = 0.25f;
                        gObj.transform.localScale *= diScale;

                        gObj.name = di.FullName;

                        gObj.AddComponent<MyDataNode>();
                        MyDataNode dn = gObj.GetComponent<MyDataNode>();
                        dn.Name = di.Name;
                        dn.Size = 0;
                        dn.FolderCount = 0;
                        dn.FullName = di.FullName;
                        dn.IsFolder = true;

                        //HANDLING FILE PERMISSION
                        FileAttributes dirAttr = di.Attributes;
                        dn.hidden = ((dirAttr & FileAttributes.Hidden) == FileAttributes.Hidden) ? true : false;
                        dn.system = ((dirAttr & FileAttributes.System) == FileAttributes.System) ? true : false;
                        dn.readOnly = ((dirAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) ? true : false;



                        if (dn.hidden)
                        {
                            gObj.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
                        }
                        if (dn.system)
                        {
                            gObj.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
                        }
                        if (dn.readOnly)
                        {
                            gObj.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.green);
                        }

                        //Storing information later to restore their original position in the helix
                        dn.ParentObject = transform.gameObject;
                        dn.CurrentPosition = gObj.transform.position;

                        var textName = Instantiate(TextMeshProPrefab, gObj.transform);
                        textName.GetComponent<TextMeshPro>().text = dn.Name;
                        textName.transform.SetParent(gObj.transform);
                        var height = gObj.transform.GetComponent<MeshRenderer>().bounds.size.y;
                        textName.transform.position = new Vector3(gObj.transform.position.x, textName.transform.position.y - height / 1.5f, gObj.transform.position.z);


                        if (!dn.system)
                        {
                            dn.FolderCount = di.GetDirectories().Length;
                        }


                        //Debug.Log($"{ di.FullName}\t\t{di.Parent}");

                    }
                    catch (UnauthorizedAccessException unAuthDir)
                    {
                        MyDataNode dn = gObj.GetComponent<MyDataNode>();
                        dn.isRestricted = true;
                        gObj.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);

                        Debug.LogWarning($"{unAuthDir.Message}");
                    }
                    i++;
                }
            }
            catch (DirectoryNotFoundException dirNotFound)
            {
                Debug.LogWarning($"{dirNotFound.Message}");
            }
            catch (UnauthorizedAccessException unAuthDir)
            {
                Debug.LogWarning($"unAuthDir: {unAuthDir.Message}");
            }
            catch (PathTooLongException longPath)
            {
                Debug.LogWarning($"{longPath.Message}");
            }
        }
    }
}
