using System;
using UnityEngine;

public class GameObjectAddComponentPackage : Package
{
    public GameObjectAddComponentPackage(){}
    public GameObjectAddComponentPackage(string json) => Deserialize(json);
    
    public string GameObjectHierarchy;
    public string ComponentName;
    
    public override void Execute()
    {
        GameObject addComponentGameObject = GameObject.Find(GameObjectHierarchy);
        if (addComponentGameObject == null)
        {
            Debug.LogWarning("GameObject not found: " + GameObjectHierarchy);
            return;
        }
        Type componentType = Type.GetType(ComponentName);
        if (componentType == null)
        {
            Debug.LogWarning("Component Type not found: " + ComponentName);
            return;
        }
        addComponentGameObject.AddComponent(componentType);
    }
}
