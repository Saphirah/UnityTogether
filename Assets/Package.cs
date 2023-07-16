using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Package
{
    public DateTime time = DateTime.Now;

    public static readonly List<Type> packages = new()
    {
        typeof(CameraTransformPackage), 
        typeof(GameObjectSerializationPackage),
        typeof(GameObjectChangeParentPackage),
        typeof(GameObjectCreatePackage),
        typeof(GameObjectDestroyPackage),
        typeof(FileDeletedPackage),
        typeof(FileRenamedPackage),
        typeof(GameObjectAddComponentPackage),
        typeof(GameObjectRemoveComponentPackage),
        typeof(ChangeScenePackage),
    };

    public Package() { }
    public Package(string json) => Deserialize(json);
    
    public virtual void Execute() {}

    public static int GetPackageIndex(Package package) => packages.IndexOf(package.GetType());
    public override string ToString() => JsonUtility.ToJson(this);
    protected virtual void Deserialize(string json) => JsonUtility.FromJsonOverwrite(json, this);
    
    public static bool IsPackageIndex(Type type, int index) => packages.IndexOf(type) == index;
}
