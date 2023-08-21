using System;
using UnityEngine;

public sealed class GameObjectCreatePackage : Package
{
    public GameObjectCreatePackage(){}
    public GameObjectCreatePackage(string json) => Deserialize(json);

    public string GameObjectHierarchy;
    public string GameObjectName;
    
    public GameObject NewParent => GameObject.Find(GameObjectHierarchy);
    
    public override void Execute()
    {
        GameObject newGameObject = new GameObject(GameObjectName) { transform = { parent = NewParent.transform } };
    }
}
