using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class FileInfo : MonoBehaviour
{
    //public string path = @"C:\Users\Nicholas\Documents\test.txt";

    public DateTime dateCreated, dateModified, dateAccessed;

    public Text fileName, fileType, location, size, folderCount, created, modified, accessed, attributes;

    public GameObject InfoPanel, DrivePanel, RestrictedAccess;
    public Text driveName, driveType, format, dSize, userfspace, fspace, usedspace;

    public void DisablePanel()
    {
        InfoPanel.SetActive(false);
        GetComponent<MyCameraControl>().canMove = true;
        GetComponent<FileSystem>().canClick = true;
    }

    public void DisableDrivePanel()
    {
        DrivePanel.SetActive(false);
        GetComponent<MyCameraControl>().canMove = true;
        GetComponent<FileSystem>().canClick = true;
    }

    public void DisableRestrictedAccessPanel()
    {
        RestrictedAccess.SetActive(false);
        GetComponent<MyCameraControl>().canMove = true;
        GetComponent<FileSystem>().canClick = true;
    }

}
