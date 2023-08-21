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

    public static SerializedProperty GetSerializedProperty(Component component, string propertyPath) => 
        new SerializedObject(component).FindProperty(propertyPath);

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

    public static Component GetComponentFromPath(string path)
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
    public static void SetSerializedPropertyValue(SerializedProperty iterator, string value)
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