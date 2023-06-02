using System;
using UnityEngine;

public class FileDeletedPackage : Package
{
    public FileDeletedPackage(){}
    public FileDeletedPackage(string json) => Deserialize(json);
    
    public string path;
}