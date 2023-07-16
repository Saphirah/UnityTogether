using System;
using UnityEngine;

public class GameObjectCreatePackage : Package
{
    public GameObjectCreatePackage(){}
    public GameObjectCreatePackage(string json) => Deserialize(json);

    public string GameObjectHierarchy;
    public string GameObjectName;
    
    public GameObject NewParent => GameObject.Find(GameObjectHierarchy);
}
