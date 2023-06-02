using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;

[ExecuteInEditMode]
public class UnityTogetherClient : MonoBehaviour
{
    public string Username = "Saphirah";
    
    private HubConnection connection;
    private List<Processor> processors;
    private static readonly ConcurrentQueue<Action> MainThreadQueue = new ConcurrentQueue<Action>();
    
    public Action<int, string, string> OnMessageReceived;
    public Action<string, string, string> OnFileReceived;
    public Action OnRender;

    public void SendPackage(Package package) => connection?.SendAsync("SendPackage", Package.GetPackageIndex(package), Username, package.ToString());
    public void SendFile(string filePath, string fileContent) => connection?.SendAsync("SendFile", Username, filePath, fileContent);

    [InitializeOnLoadMethod]
    private static void Initialize() => EditorApplication.update += ExecuteActions;
    public static void Enqueue(Action action) => MainThreadQueue.Enqueue(action);
    
    private static void ExecuteActions()
    {
        while (MainThreadQueue.TryDequeue(out var action))
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
    }

    private void OnEnable()
    {
        AssemblyReloadEvents.afterAssemblyReload += Connect;
        
        if (processors != null) return;
 
        processors = new List<Processor>()
        {
            new CameraDrawer(this),
            new GameObjectSync(this),
            new FileSync(this)
        };
    }
    
    private void OnDisable()
    {
        AssemblyReloadEvents.afterAssemblyReload -= Connect;
    }

    public void Connect() => ConnectToServer();


    public async Task ConnectToServer()
    {
        connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/myhub")
            .Build();

        connection.On<int, string, string>("ReceivePackage", (packageID, username, package) =>
        {
            Debug.Log("Received message: " + package);
            OnMessageReceived?.Invoke(packageID, package, username);
        });

        connection.On<string, string, string>("ReceiveFile", (username, filePath, fileData) =>
        {
            Debug.Log("Received file");
            OnFileReceived?.Invoke(username, filePath, fileData);
        });
        
        try
        {
            await connection.StartAsync();
            Debug.Log("Connected to hub");
        }
        catch (Exception ex)
        {
            Debug.Log("Error connecting to hub: " + ex.Message);
            Disconnect();
        }
    }
    
    public bool IsConnected() => connection is {State: HubConnectionState.Connected};
    public void Disconnect() => connection.StopAsync();
    private void OnDrawGizmos() => OnRender?.Invoke();
}

[CustomEditor(typeof(UnityTogetherClient))]
public class UnityTogetherClientEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UnityTogetherClient client = (UnityTogetherClient)target;

        if(client.IsConnected())
            EditorGUILayout.HelpBox("Connected to server", MessageType.Info);
        if (!client.IsConnected())
        {
            if (GUILayout.Button("Connect"))
                client.ConnectToServer();
        }
        else
        {
            if (GUILayout.Button("Disconnect"))
                client.Disconnect();
        }
        if (GUILayout.Button("Start Server"))
        {
            //Start server in a CMD Console
            string path = Application.dataPath + "/../UnityTogetherServer/net6.0/UnityTogetherServer.exe";
            
            System.Diagnostics.Process.Start(path);
        }
    }
}
