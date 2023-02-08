using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public abstract class Communication : MonoBehaviour
{
    [Header("Server Config")]
    public string IP = "";
    public int Port = 8085;
    [Range(1, 30)]
    public int ticksPerSecond = 10;

    [Header("User Config")]
    public string Username = "";
    
    private string inputStream = "";

    public abstract bool IsConnected();
    protected abstract Stream GetStream();
    
    //Package ID, Content, UserID
    public Action<int, string, string> OnMessageReceived;
    public Action OnRender;
    
    public Action OnConnect;
    
    public TcpClient client;
    
    public static Communication Instance { get; protected set; }

    private void OnDrawGizmos() => OnRender?.Invoke();

    public virtual void SendPackage(Package package)
    {
        if (!IsConnected()) return;
        Stream stream = GetStream();
        List<byte> data = Encoding.ASCII.GetBytes(Package.GetPackageIndex(package) + "#" + Username + "#" + package + ";").ToList();
        
        stream.Write(data.ToArray(), 0, data.Count);
    }

    protected virtual IEnumerator ListenToClient(TcpClient client)
    { 
        NetworkStream ns = client.GetStream();
        while (client is {Connected: true})
        {
            yield return new WaitUntil(() => ns.DataAvailable);
            byte[] msgBuffer = new byte[50000];
            Task<int> readTask = ns.ReadAsync(msgBuffer, 0, msgBuffer.Length);
            msgBuffer = msgBuffer.Take(readTask.Result).ToArray();
            string msg = Encoding.ASCII.GetString(msgBuffer);
            
            string[] messages = msg.Split(';');
            foreach (string message in messages)
            {
                if (message == "") continue;
                string[] seperatedMessage = message.Split('#');
                int index = int.Parse(seperatedMessage[0]);
                string user = seperatedMessage[1];
                //Debug.Log(Username + " received Message from Server: " + seperatedMessage[2]);
                OnMessageReceived?.Invoke(index, seperatedMessage[2], user);
            }
        }
    }
}