using System;
using UnityEngine;

public class FileRenamedPackage : Package
{
    public FileRenamedPackage(){}
    public FileRenamedPackage(string json) => Deserialize(json);
    
    public string oldPath;
    public string newPath;
}
