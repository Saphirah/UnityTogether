using System;
using System.IO;
using UnityEngine;

public class FileSync : Processor
{
    private FileSystemWatcher watcher;
    private string dataPath;
    
    public FileSync(UnityTogetherClient com) : base(com)
    {
        dataPath = Application.dataPath;
        
        watcher = new FileSystemWatcher(dataPath);
        watcher.IncludeSubdirectories = true;
        
        watcher.Changed += OnFileChanged;
        watcher.Created += OnFileChanged;
        watcher.Deleted += OnFileDeleted;
        watcher.Renamed += OnFileRenamed;
        
        watcher.EnableRaisingEvents = true;
        
        com.OnFileReceived += OnFileReceived;
    }

    protected void OnFileReceived(string username, string filePath, string fileContent)
    {
        Debug.Log("File changed: " + GetRelativePath(filePath));
        byte[] fileBytes = Convert.FromBase64String(fileContent);
        PerformSafeFileOperation(() => 
        {
            File.WriteAllBytes(dataPath + filePath, fileBytes);
        });
    }

    protected override void OnMessageReceived(int index, string msg, string userID)
    {
        if (Package.IsPackageIndex(typeof(FileDeletedPackage), index))
        {
            FileDeletedPackage package = new FileDeletedPackage(msg);
            PerformSafeFileOperation(() => 
            {
                if (File.Exists(dataPath + package.path))
                    File.Delete(dataPath + package.path);
            });
        }

        if (Package.IsPackageIndex(typeof(FileRenamedPackage), index))
        {
            FileRenamedPackage package = new FileRenamedPackage(msg);
            PerformSafeFileOperation(() => 
            {
                if (File.Exists(dataPath + package.oldPath))
                    File.Move(dataPath + package.oldPath, dataPath + package.newPath);
            });
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Debug.Log("File changed: " + GetRelativePath(e.FullPath));
        communication.SendFile(
            GetRelativePath(e.FullPath), 
            Convert.ToBase64String(File.ReadAllBytes(e.FullPath))
        );
    }
    
    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        Debug.Log("File deleted: " + GetRelativePath(e.FullPath));
        communication.SendPackage(new FileDeletedPackage()
        {
            path = GetRelativePath(e.FullPath)
        });
    }
    
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        Debug.Log("File renamed: " + GetRelativePath(e.FullPath));
        communication.SendPackage(new FileRenamedPackage()
        {
            oldPath = GetRelativePath(e.OldFullPath),
            newPath = GetRelativePath(e.FullPath)
        });
    }
    
    private void PerformSafeFileOperation(Action action)
    {
        watcher.EnableRaisingEvents = false;
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
            watcher.EnableRaisingEvents = true;
        }
    }
    
    private string GetRelativePath(string path) => path.Replace(dataPath, "");
}