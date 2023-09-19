using System;
using System.IO;
using UnityEngine;

public sealed class FileDeletedPackage : FilePackage
{
    public FileDeletedPackage() { }
    public FileDeletedPackage(string json) => Deserialize(json);
    
    public string path;
    public override void Execute()
    {
        PerformSafeFileOperation(() => 
        {
            
            if (File.Exists(dataPath + path))
            {
                File.Delete(dataPath + path);
                FileSync.RefreshAsset(dataPath + path);
            }
        });
    }
}