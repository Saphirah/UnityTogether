using System;
using UnityEngine;

public class CameraTransformPackage : Package
{
    public CameraTransformPackage(){}
    public CameraTransformPackage(string json) => Deserialize(json);
    
    public Vector3 position;
    public Quaternion rotation;
    public PositionRotation positionRotation => new(){position = position, rotation = rotation};
    
    public override void Execute() => CameraDrawer.UpdateTransform(username, positionRotation);
}

public struct PositionRotation
{
    public Vector3 position;
    public Quaternion rotation;
}
