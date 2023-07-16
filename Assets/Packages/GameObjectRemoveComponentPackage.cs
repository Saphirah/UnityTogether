using System;
using UnityEngine;

public class GameObjectRemoveComponentPackage : Package
{
    public GameObjectRemoveComponentPackage(){}
    public GameObjectRemoveComponentPackage(string json) => Deserialize(json);
    
    public string GameObjectHierarchy;
    public string ComponentName;
    public int ComponentIndex;
}
