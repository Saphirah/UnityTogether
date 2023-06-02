using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraDrawer : Processor
{
    private Mesh mesh;
    private Material material;
    private CameraTransformPackage cameraTransformPackage = new();
    
    public CameraDrawer(UnityTogetherClient com) : base(com)
    {
        mesh = Resources.Load<Mesh>("CameraMesh");
        material = Resources.Load<Material>("CameraMaterial");
        com.OnRender += Render;
        com.OnRender += Update;
    }
    
    private Dictionary<string, PositionRotation> clients = new();
    private Dictionary<string, PositionRotation> currentTransform = new();

    protected override void OnMessageReceived(int index, string msg, string userID)
    {
        if (index != 0) return;
        CameraTransformPackage package = new CameraTransformPackage(msg);
        clients[userID] = package.positionRotation;
        if(!currentTransform.ContainsKey(userID))
            currentTransform.Add(userID, new PositionRotation());
    }
    
    public void Update()
    {
        if (cameraTransformPackage.position == SceneView.lastActiveSceneView.camera.transform.position && cameraTransformPackage.rotation == SceneView.lastActiveSceneView.camera.transform.rotation) return;
        Transform transform = SceneView.lastActiveSceneView.camera.transform;
        cameraTransformPackage = new CameraTransformPackage
        {
            position = transform.position,
            rotation = transform.rotation
        };
        communication.SendPackage(cameraTransformPackage);
        Debug.Log("Sent camera transform");
    }
    
    public void Render()
    {
        foreach (string client in clients.Keys)
        {
            if(communication.Username == client) continue;
            //Interpolation
            PositionRotation positionRotation = currentTransform[client];
            positionRotation.position = Vector3.Lerp(positionRotation.position, clients[client].position, Mathf.Clamp01(Time.deltaTime * 10));
            positionRotation.rotation = Quaternion.Lerp(positionRotation.rotation, clients[client].rotation, Mathf.Clamp01(Time.deltaTime * 10));
            currentTransform[client] = positionRotation;
            
            Gizmos.DrawMesh(mesh, 0, positionRotation.position, positionRotation.rotation, Vector3.one);
            Gizmos.DrawRay(positionRotation.position, positionRotation.rotation * Vector3.forward * 50);
        }
    }
}