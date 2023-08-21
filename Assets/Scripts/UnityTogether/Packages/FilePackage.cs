using System;
using System.IO;
using UnityEngine;

public abstract class FilePackage : Package
{
    protected string dataPath;
    protected readonly FileSystemWatcher fileWatcher;

    protected FilePackage()
    {
        dataPath = Application.dataPath;
        fileWatcher = new FileSystemWatcher(dataPath);
        fileWatcher.IncludeSubdirectories = true;
        fileWatcher.EnableRaisingEvents = true;
    }

    protected FilePackage(string json) => Deserialize(json);
    
    protected void PerformSafeFileOperation(Action action)
    {
        fileWatcher.EnableRaisingEvents = false;
        try
        {
            action();
        }
        catch (Exception e)
        {
            Debug.Log($"Error while performing file operation: {e}");
        }
        finally
        {
            fileWatcher.EnableRaisingEvents = true;
        }
    }
}