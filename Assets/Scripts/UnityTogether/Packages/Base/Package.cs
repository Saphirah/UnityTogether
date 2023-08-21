using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class Package
{
    public DateTime time = DateTime.Now;
    public string username = UnityTogetherClient.Instance.Username;

    public static readonly List<Type> packages = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.IsSubclassOf(typeof(Package)) && !t.IsAbstract)
        .ToList();

    public Package() { }
    public Package(string json) => Deserialize(json); 

    public abstract void Execute();

    public static int GetPackageIndex(Package package) => packages.IndexOf(package.GetType());
    public static Type GetPackageType(int index) => packages[index];
    public override string ToString() => JsonUtility.ToJson(this);
    protected virtual void Deserialize(string json) => JsonUtility.FromJsonOverwrite(json, this);
    
    public static bool IsPackageIndex(Type type, int index) => packages.IndexOf(type) == index;
}
