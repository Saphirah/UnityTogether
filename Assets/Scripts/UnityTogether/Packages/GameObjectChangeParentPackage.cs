using System;
using UnityEngine;

public class GameObjectChangeParentPackage : Package
{
    public GameObjectChangeParentPackage(){}
    public GameObjectChangeParentPackage(string json) => Deserialize(json);

    public string GameObjectHierarchy;
    public string NewParentHierarchy;
    
    public GameObject GameObject => GameObject.Find(GameObjectHierarchy);
    public GameObject NewParent => GameObject.Find(NewParentHierarchy);
    
    public override void Execute() => GameObject.transform.SetParent(NewParent?.transform);
}
