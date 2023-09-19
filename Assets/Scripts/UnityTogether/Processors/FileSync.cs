using System;
using System.Collections.Generic;
using System.IO;
using ParrelSync;
using UnityEditor;
using UnityEngine;

public class FileSync : Processor
{
    private FileSystemWatcher watcher;
    private static string dataPath;
    
    private static List<string> changedFiles = new List<string>();
    
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
        try
        {
            Debug.Log("File changed: " + GetRelativePath(filePath));
            byte[] newFileBytes = Convert.FromBase64String(fileContent);
            try
            {
                byte[] fileBytes = File.ReadAllBytes(dataPath + filePath);
                Debug.Log("Assets/" + filePath);
                if (newFileBytes == fileBytes) return;
            }
            catch (Exception e) { }

            PerformSafeFileOperation(() =>
            {
                changedFiles.Add(filePath);
                File.WriteAllBytes(dataPath + filePath, newFileBytes);
                if (!filePath.EndsWith(".cs") && !filePath.EndsWith(".unity"))
                    RefreshAsset(filePath);
            });
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.Name.Contains(".unity")) return;
        string relativePath = GetRelativePath(e.FullPath);
        if (changedFiles.Contains(relativePath))
        {
            changedFiles.Remove(relativePath);
            return;
        }
        Debug.Log("File changed: " + relativePath);
        communication.SendFile(
            relativePath, 
            Convert.ToBase64String(File.ReadAllBytes(e.FullPath))
        );
    }
    
    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        Debug.Log("File deleted: " + GetRelativePath(e.FullPath));
        communication.SendPackage(new FileDeletedPackage() { path = GetRelativePath(e.FullPath) });
    }

    public static void RefreshAsset(string absolutePath)
    {
        EditorApplication.delayCall += () =>
        {
            try
            {
                AssetDatabase.ImportAsset("Assets" + GetRelativePath(absolutePath));
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        };
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
    
    public void PerformSafeFileOperation(Action action)
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
    
    private static string GetRelativePath(string path) => path.Replace(dataPath, "");
}