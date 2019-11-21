using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class FileInfo : MonoBehaviour
{
    public string path = @"C:\Users\Nicholas\Documents\test.txt";

    public DateTime dateCreated, dateModified, dateAccessed;
    public string fileAttributes, extension, location = @"C:\Users\Nicholas\Documents";

    public Text fileName, fileType, locationText, size, created, modified, accessed, attributes;
    public InputField fileNameInput;

    // Start is called before the first frame update
    void Start()
    {
        extension = Path.GetExtension(path);
        dateCreated = File.GetCreationTime(path);
        dateModified = File.GetLastWriteTime(path);
        dateAccessed = File.GetLastAccessTime(path);
        fileAttributes = File.GetAttributes(path).ToString();

        fileName.text = Path.GetFileName(path) + " Properties";
        fileType.text = extension;
        locationText.text = Path.GetDirectoryName(path);
        size.text = "32 bytes";
        created.text = dateCreated.ToString("g");
        modified.text = dateModified.ToString("g");
        accessed.text = dateAccessed.ToString("g");
        attributes.text = fileAttributes;

        fileNameInput.text = Path.GetFileName(path);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
