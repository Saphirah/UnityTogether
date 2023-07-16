using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Unity.Serialization.Json;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameObjectSync : Processor
{
    private const string SEPERATOR = "$&%-/!@#";
    
    public GameObjectSync(UnityTogetherClient com) : base(com)
    {
        Undo.postprocessModifications += OnPostprocessModifications;
    }

    private UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
    {
        foreach (UndoPropertyModification modification in modifications)
        {
            if (modification.currentValue.target is not Component component) continue;
            
            SerializedProperty property = GetSerializedProperty(component, modification.currentValue.propertyPath);
            communication.SendPackage(new GameObjectSerializationPackage()
            {
                ComponentName = component.GetType().AssemblyQualifiedName,
                ComponentIndex = component.GetComponents(component.GetType()).ToList().IndexOf(component),
                GameObjectHierarchy = GetPath(component.transform),
                Value = GetSerializedPropertyValue(property)?.ToString(),
                VariableName = modification.currentValue.propertyPath
            });
        }

        return modifications;
    }

    private SerializedProperty GetSerializedProperty(Component component, string propertyPath)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        return serializedObject.FindProperty(propertyPath);
    }

    protected override void OnMessageReceived(int index, string msg, string userID)
    {
        if(UnityTogetherClient.Instance.Username == userID) return;
        ObjectChangeEventsExample.SetActive(false);
        switch (index)
        {
            //Changed SerializedProperty
            case 1:
                GameObjectSerializationPackage serializationPackage = new GameObjectSerializationPackage(msg);
                GameObject gameObject = GameObject.Find(serializationPackage.GameObjectHierarchy);
                if (gameObject == null) return;
                Type serializedPropertyComponentType = Type.GetType(serializationPackage.ComponentName);
                Component[] components = gameObject.GetComponents(serializedPropertyComponentType);
                if (components.Length <= serializationPackage.ComponentIndex)
                {
                    Debug.LogWarning("Component at Index not found: " + serializationPackage.ComponentName + " " + serializationPackage.ComponentIndex);
                    return;
                }
                Component component = components[serializationPackage.ComponentIndex];
                if (component == null) return;
                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty iterator = serializedObject.GetIterator();
                iterator.Next(true);
                while (iterator.Next(true))
                {
                    string path = iterator.propertyPath;
                    if (path != serializationPackage.VariableName) continue;
                    SetSerializedPropertyValue(iterator, serializationPackage.Value);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }
                break;  
            //Changed Parent
            case 2:
                GameObjectChangeParentPackage changeParentPackage = new GameObjectChangeParentPackage(msg);
                changeParentPackage.GameObject.transform.SetParent(changeParentPackage.NewParent?.transform);
                break;
            //Created gameObject
            case 3:
                GameObjectCreatePackage createPackage = new GameObjectCreatePackage(msg);
                GameObject newGameObject = new GameObject(createPackage.GameObjectName);
                newGameObject.transform.parent = createPackage.NewParent.transform;
                break;
            //Destroy GameObject
            case 4:
                GameObjectDestroyPackage destroyPackage = new GameObjectDestroyPackage(msg);
                GameObject.DestroyImmediate(destroyPackage.GameObject);
                break;
            //Add Component
            case 7:
                GameObjectAddComponentPackage addComponentPackage = new GameObjectAddComponentPackage(msg);
                GameObject addComponentGameObject = GameObject.Find(addComponentPackage.GameObjectHierarchy);
                if (addComponentGameObject == null)
                {
                    Debug.LogWarning("GameObject not found: " + addComponentPackage.GameObjectHierarchy);
                    return;
                }
                Type componentType = Type.GetType(addComponentPackage.ComponentName);
                if (componentType == null)
                {
                    Debug.LogWarning("Component Type not found: " + addComponentPackage.ComponentName);
                    return;
                }
                addComponentGameObject.AddComponent(componentType);
                break;
            //Remove Component
            case 8:
                GameObjectRemoveComponentPackage removeComponentPackage = new GameObjectRemoveComponentPackage(msg);
                GameObject removeComponentGameObject = GameObject.Find(removeComponentPackage.GameObjectHierarchy);
                if (removeComponentGameObject == null)
                {
                    Debug.LogWarning("GameObject not found: " + removeComponentPackage.GameObjectHierarchy);
                    return;
                }
                Type removeComponentType = Type.GetType(removeComponentPackage.ComponentName);
                if (removeComponentType == null)
                {
                    Debug.LogWarning("Component Type not found: " + removeComponentPackage.ComponentName);
                    return;
                }
                Component[] removeComponents = removeComponentGameObject.GetComponents(removeComponentType);
                if (removeComponents.Length <= removeComponentPackage.ComponentIndex)
                {
                    Debug.LogWarning("Component at Index not found: " + removeComponentPackage.ComponentName + " " + removeComponentPackage.ComponentIndex);
                    return;
                }
                Object.DestroyImmediate(removeComponents[removeComponentPackage.ComponentIndex]);
                break;
            //Changed Scene
            case 9:
                ChangeScenePackage changeScenePackage = new ChangeScenePackage(msg);
                EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene(changeScenePackage.SceneName);
                break;
        }
        ObjectChangeEventsExample.SetActive(true);
    }

    public static string GetPath(Transform current)
    {
        if (current.parent == null)
            return "/" + current.name;
        return GetPath(current.parent) + "/" + current.name;
    }

    private static string GetPath(Component component)
    {
        return component.gameObject.GetPath() + SEPERATOR +
               component.gameObject.GetComponents(component.GetType()).ToList().IndexOf(component) + SEPERATOR +
               component.GetType().AssemblyQualifiedName;
    }

    private Component GetComponentFromPath(string path)
    {
        string[] split = path.Split(SEPERATOR);
        GameObject gameObject = GameObject.Find(split[0]);
        if (gameObject == null) return null;
        Type componentType = Type.GetType(split[2]);
        if (componentType == null) return null;
        Component[] components = gameObject.GetComponents(componentType);
        return components.Length <= int.Parse(split[1]) ? null : components[int.Parse(split[1])];
    }

    public static object GetSerializedPropertyValue(SerializedProperty iterator)
    {
        try
        {
            return iterator.propertyType switch
            {
                SerializedPropertyType.Vector3 => iterator.vector3Value,
                SerializedPropertyType.Quaternion => iterator.quaternionValue,
                SerializedPropertyType.Float => iterator.floatValue,
                SerializedPropertyType.Integer => iterator.longValue,
                SerializedPropertyType.Boolean => iterator.boolValue,
                SerializedPropertyType.String => iterator.stringValue,
                SerializedPropertyType.ObjectReference => 
                    AssetDatabase.Contains(iterator.objectReferenceValue) ? 
                        AssetDatabase.GetAssetPath(iterator.objectReferenceValue) : 
                        iterator.objectReferenceValue is Component component ? 
                            GetPath(component) : 
                            iterator.objectReferenceValue is GameObject gameObject ? 
                                GetPath(gameObject.transform) : "",
            SerializedPropertyType.Enum => iterator.enumValueIndex,
                SerializedPropertyType.Color => iterator.colorValue,
                SerializedPropertyType.LayerMask => iterator.intValue,
                SerializedPropertyType.Vector2 => iterator.vector2Value,
                SerializedPropertyType.Vector4 => iterator.vector4Value,
                SerializedPropertyType.Rect => iterator.rectValue,
                SerializedPropertyType.ArraySize => iterator.intValue,
                SerializedPropertyType.Character => iterator.intValue,
                SerializedPropertyType.AnimationCurve => iterator.animationCurveValue,
                SerializedPropertyType.Bounds => iterator.boundsValue,
                SerializedPropertyType.ExposedReference => iterator.exposedReferenceValue,
                SerializedPropertyType.FixedBufferSize => iterator.intValue,
                SerializedPropertyType.Vector2Int => iterator.vector2IntValue,
                SerializedPropertyType.Vector3Int => iterator.vector3IntValue,
                SerializedPropertyType.RectInt => iterator.rectIntValue,
                SerializedPropertyType.BoundsInt => iterator.boundsIntValue,
                SerializedPropertyType.ManagedReference => iterator.managedReferenceValue,
                SerializedPropertyType.Hash128 => iterator.hash128Value,
                _ => null
            };
        }
        catch (Exception e)
        {
            return null;
        }
    }

    //Now do the GetSerializedPropertyValue but with set
    private void SetSerializedPropertyValue(SerializedProperty iterator, string value)
    {
        switch (iterator.propertyType)
        {
            case SerializedPropertyType.Vector3:
                iterator.vector3Value = StringToVector3(value);
                break;
            case SerializedPropertyType.Quaternion:
                iterator.quaternionValue = StringToQuaternion(value);
                break;
            case SerializedPropertyType.Float:
                iterator.floatValue = float.Parse(value);
                break;
            case SerializedPropertyType.Integer:
                iterator.longValue = long.Parse(value);
                break;
            case SerializedPropertyType.Boolean:
                iterator.boolValue = Convert.ToBoolean(value);
                break;
            case SerializedPropertyType.String:
                iterator.stringValue = value;
                break;
            case SerializedPropertyType.ObjectReference:
                Object obj = null;
                if (value.Contains(SEPERATOR))
                    obj = GetComponentFromPath(value);
                if(obj == null)
                    obj = GameObject.Find(value);
                if(obj == null)
                    obj = AssetDatabase.LoadAssetAtPath<Object>(value);
                iterator.objectReferenceValue = obj;
                break;
            case SerializedPropertyType.Enum:
                iterator.enumValueIndex = int.Parse(value);
                break;
            case SerializedPropertyType.Color:
                iterator.colorValue = StringToColor(value);
                break;
            case SerializedPropertyType.Generic:
                //iterator.objectReferenceValue = (UnityEngine.Object) value;
                break;
            case SerializedPropertyType.LayerMask:
                iterator.intValue = int.Parse(value);
                break;
            case SerializedPropertyType.Vector2:
                iterator.vector2Value = StringToVector2(value);
                break;
            case SerializedPropertyType.Vector4:
                iterator.vector4Value = StringToVector4(value);
                break;
            case SerializedPropertyType.Rect:
                //iterator.rectValue = (Rect) value;
                break;
            case SerializedPropertyType.ArraySize:
                iterator.intValue = int.Parse(value);
                break;
            case SerializedPropertyType.Character:
                iterator.intValue = int.Parse(value);
                break;
            case SerializedPropertyType.AnimationCurve:
                //iterator.animationCurveValue = (AnimationCurve) value;
                break;
            case SerializedPropertyType.Bounds:
                //iterator.boundsValue = (Bounds) value;
                break;
            case SerializedPropertyType.Gradient:
                //iterator.objectReferenceValue = (UnityEngine.Object) value;
                break;
            case SerializedPropertyType.ExposedReference:
                //iterator.exposedReferenceValue = (UnityEngine.Object) value;
                break;
            case SerializedPropertyType.FixedBufferSize:
                iterator.intValue = int.Parse(value);
                break;
            case SerializedPropertyType.Vector2Int:
                iterator.vector2IntValue = StringToVector2Int(value);
                break;
            case SerializedPropertyType.Vector3Int:
                iterator.vector3IntValue = StringToVector3Int(value);
                break;
            case SerializedPropertyType.RectInt:
                //iterator.rectIntValue = (RectInt) value;
                break;
            case SerializedPropertyType.BoundsInt:
                //iterator.boundsIntValue = (BoundsInt) value;
                break;
            case SerializedPropertyType.ManagedReference:
                //iterator.managedReferenceValue = (UnityEngine.Object) value;
                break;
            case SerializedPropertyType.Hash128:
                //iterator.hash128Value = (Hash128) value;
                break;
            default:
                break;
        }
    }

    public static Vector3 StringToVector3(string sVector)
    {
        string[] sArray = sVector.Replace("(", "").Replace(")","").Split(',');
        
        return new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));
    }
    
    public static Quaternion StringToQuaternion(string sVector)
    {
        string[] sArray = sVector.Replace("(", "").Replace(")","").Split(',');
        
        return new Quaternion(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));
    }
    
    public static Vector2 StringToVector2(string sVector)
    {
        string[] sArray = sVector.Replace("(", "").Replace(")","").Split(',');
        
        return new Vector2(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]));
    }
    
    public static Vector4 StringToVector4(string sVector)
    {
        string[] sArray = sVector.Replace("(", "").Replace(")","").Split(',');
        
        return new Vector4(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));
    }
    
    public static Color StringToColor(string sVector)
    {
        // Convert the string back to a 32-bit integer
        Debug.Log(sVector);
        uint colorValue = uint.Parse(sVector);
        
        return new Color32(
            (byte)((colorValue >> 24) & 255),
            (byte)((colorValue >> 16) & 255),
            (byte)((colorValue >> 8) & 255),
            (byte)(colorValue & 255)
        );
    }
    
    public static Vector2Int StringToVector2Int(string sVector)
    {
        string[] sArray = sVector.Replace("(", "").Replace(")","").Split(',');
        
        return new Vector2Int(
            int.Parse(sArray[0]),
            int.Parse(sArray[1]));
    }
    
    public static Vector3Int StringToVector3Int(string sVector)
    {
        string[] sArray = sVector.Replace("(", "").Replace(")","").Split(',');
        
        return new Vector3Int(
            int.Parse(sArray[0]),
            int.Parse(sArray[1]),
            int.Parse(sArray[2]));
    }
}