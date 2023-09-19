using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FileRenamedPackage : FilePackage
{
    public FileRenamedPackage(){}
    public FileRenamedPackage(string json) => Deserialize(json);
    
    public string oldPath;
    public string newPath;
    
    public override void Execute()
    {
        PerformSafeFileOperation(() => 
        {
            if (File.Exists(dataPath + oldPath))
            {
                File.Move(dataPath + oldPath, dataPath + newPath);
                FileSync.RefreshAsset(dataPath + oldPath);
                FileSync.RefreshAsset(dataPath + newPath);
            }
        });
    }
}