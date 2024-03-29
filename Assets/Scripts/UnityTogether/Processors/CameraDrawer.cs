﻿using System.Collections.Generic;
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
    
    private static Dictionary<string, PositionRotation> clients = new();
    private static Dictionary<string, PositionRotation> currentTransform = new();

    public static void UpdateTransform(string username, PositionRotation transform)
    {
        clients[username] = transform;
        if(!currentTransform.ContainsKey(username))
            currentTransform.Add(username, new PositionRotation());
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