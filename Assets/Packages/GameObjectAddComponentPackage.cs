using System;
using UnityEngine;

public class GameObjectAddComponentPackage : Package
{
    public GameObjectAddComponentPackage(){}
    public GameObjectAddComponentPackage(string json) => Deserialize(json);
    
    public string GameObjectHierarchy;
    public string ComponentName;
}
