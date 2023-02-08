using System;
using UnityEngine;

public class GameObjectCreatePackage : Package
{
    public GameObjectCreatePackage(){}
    public GameObjectCreatePackage(string json) => Deserialize(json);

    public string NewGameObject;
    public string NewParentHierarchy;
    
    public GameObject NewParent => GameObject.Find(NewParentHierarchy);
}
