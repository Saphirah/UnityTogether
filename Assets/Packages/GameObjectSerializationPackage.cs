using System;
using UnityEditor;
using UnityEngine;

public class GameObjectSerializationPackage : Package
{
    public GameObjectSerializationPackage(){}
    public GameObjectSerializationPackage(string json) => Deserialize(json);
    
    public string GameObjectHierarchy;
    public string ComponentName;
    public int ComponentIndex;
    public string VariableName;
    public string Value;
    
    public override void Execute()
    {
        GameObject gameObject = GameObject.Find(GameObjectHierarchy);
        if (gameObject == null) return;
        Type serializedPropertyComponentType = Type.GetType(ComponentName);
        Component[] components = gameObject.GetComponents(serializedPropertyComponentType);
        if (components.Length <= ComponentIndex)
        {
            Debug.LogWarning("Component at Index not found: " + ComponentName + " " + ComponentIndex);
            return;
        }
        Component component = components[ComponentIndex];
        if (component == null) return;
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty iterator = serializedObject.GetIterator();
        iterator.Next(true);
        while (iterator.Next(true))
        {
            string path = iterator.propertyPath;
            if (path != VariableName) continue;
            GameObjectSync.SetSerializedPropertyValue(iterator, Value);
            serializedObject.ApplyModifiedProperties();
            break;
        }
    }
}
