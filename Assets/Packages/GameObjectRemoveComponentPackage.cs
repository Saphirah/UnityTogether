using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameObjectRemoveComponentPackage : Package
{
    public GameObjectRemoveComponentPackage(){}
    public GameObjectRemoveComponentPackage(string json) => Deserialize(json);
    
    public string GameObjectHierarchy;
    public string ComponentName;
    public int ComponentIndex;
    public override void Execute()
    {
        GameObject removeComponentGameObject = GameObject.Find(GameObjectHierarchy);
        if (removeComponentGameObject == null)
        {
            Debug.LogWarning("GameObject not found: " + GameObjectHierarchy);
            return;
        }
        Type removeComponentType = Type.GetType(ComponentName);
        if (removeComponentType == null)
        {
            Debug.LogWarning("Component Type not found: " + ComponentName);
            return;
        }
        Component[] removeComponents = removeComponentGameObject.GetComponents(removeComponentType);
        if (removeComponents.Length <= ComponentIndex)
        {
            Debug.LogWarning("Component at Index not found: " + ComponentName + " " + ComponentIndex);
            return;
        }
        Object.DestroyImmediate(removeComponents[ComponentIndex]);
    }
}
