using System;
using UnityEngine;

public class GameObjectSerializationPackage : Package
{
    public GameObjectSerializationPackage(){}
    public GameObjectSerializationPackage(string json) => Deserialize(json);
    
    public string GameObjectHierarchy;
    public string ComponentName;
    public string VariableName;
    public string Value;
}
