using System;
using System.Collections.Generic;
using UnityEngine;

public class Package
{
    public DateTime time = DateTime.Now;

    public static List<Type> packages = new()
    {
        typeof(CameraTransformPackage), 
        typeof(GameObjectSerializationPackage),
        typeof(GameObjectChangeParentPackage),
        typeof(GameObjectCreatePackage),
        typeof(GameObjectDestroyPackage)
    };

    public Package(){}
    public Package(string json) => Deserialize(json);

    public static int GetPackageIndex(Package package) => packages.IndexOf(package.GetType());
    public override string ToString() => JsonUtility.ToJson(this);
    protected virtual void Deserialize(string json) => JsonUtility.FromJsonOverwrite(json, this);
}
