using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Client : Communication
{
    public Action OnUpdate;
    
    private CameraDrawer drawer;
    private GameObjectSync gameObjectSync;
    private FileSync fileSync;

    public bool AutoConnect = true;

    [InitializeOnLoadMethod]
    private static void AutoConnectToServer()
    {
        Client client = FindObjectOfType<Client>();
        if(!client) return;
        if(!client.AutoConnect) return;
        client.ConnectToServer();
        AssemblyReloadEvents.beforeAssemblyReload += () =>
        {
            client.client?.Close();
        };
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(IPAddress.Parse(IP), Port);
            EditorCoroutineUtility.StartCoroutine(ListenToClient(client), this);
            EditorCoroutineUtility.StartCoroutine(UpdateLoop(), this);
            OnConnect?.Invoke();
            Instance = this;
        } catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void Disconnect() => client.Close();

    public IEnumerator UpdateLoop()
    {
        Debug.Log("Update Loop started");
        //drawer ??= new CameraDrawer(this);
        //gameObjectSync ??= new GameObjectSync(this);
        //fileSync ??= new FileSync(this);
        while (IsConnected())
        {
            yield return new WaitForSecondsEditor(1f / ticksPerSecond);
            OnUpdate?.Invoke();
            Debug.Log("Updating");
        }
        Debug.Log("Update Loop ended");
    }

    public override bool IsConnected() => client is {Connected: true};
    protected override Stream GetStream() => client.GetStream();
}

[CustomEditor(typeof(Client))]
public class ClientEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Client client = (Client)target;

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
            
            System.Diagnostics.Process.Start(path, client.IP + " " + client.Port);
        }

    }
}
