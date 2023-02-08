using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEngine;

public class GameObjectSync : Processor
{
    private GameObject[] selectedGameObjects = new GameObject[0];
    private List<Dictionary<KeyValuePair<Component, string>, object>> gameObjectProperties = new();

    public GameObjectSync(Client com) : base(com)
    {
        com.OnUpdate += Update;
    }

    protected override void OnMessageReceived(int index, string msg, string userID)
    {
        switch (index)
        {
            case 1:
                //Debug.Log(msg);
                GameObjectSerializationPackage serializationPackage = new GameObjectSerializationPackage(msg);
                GameObject gameObject = GameObject.Find(serializationPackage.GameObjectHierarchy);
                if (gameObject == null) return;
                //Debug.Log("Gameobject found");
                Component component = gameObject.GetComponents<Component>().ToList()
                    .Find(c => c.GetType().Name == serializationPackage.ComponentName);
                if (component == null) return;
                //Debug.Log("Component found " + component.GetType().Name);
                Type type = component.GetType();
                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty iterator = serializedObject.GetIterator();
                iterator.Next(true);
                while (iterator.NextVisible(false))
                {
                    if (iterator.name == serializationPackage.VariableName)
                    {
                        //Debug.Log("Property found " + iterator.name);
                        //Debug.Log("Value " + serializationPackage.Value);
                        SetSerializedPropertyValue(iterator, serializationPackage.Value);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }
                break;  
            case 2:
                if(Communication.Instance.Username == userID) return;
                GameObjectChangeParentPackage changeParentPackage = new GameObjectChangeParentPackage(msg);
                changeParentPackage.GameObject.transform.SetParent(changeParentPackage.NewParent?.transform);
                break;
            case 3:
                if(Communication.Instance.Username == userID) return;
                GameObjectCreatePackage createPackage = new GameObjectCreatePackage(msg);
                GameObject newGameObject = JsonSerialization.FromJson<GameObject>(msg);
                newGameObject.transform.parent = createPackage.NewParent.transform;
                break;
            case 4:
                if(Communication.Instance.Username == userID) return;
                GameObjectDestroyPackage destroyPackage = new GameObjectDestroyPackage(msg);
                GameObject.DestroyImmediate(destroyPackage.GameObject);
                break;
        }
    }

    private void Update()
    {
        if (!Selection.gameObjects.Any()) return;
        
        //Reset Selection
        if (!Selection.gameObjects.ToHashSet().SetEquals(selectedGameObjects))
        {
            selectedGameObjects = Selection.gameObjects;
            gameObjectProperties.Clear();
            for (int i = 0; i < selectedGameObjects.Length; i++)
            {
                Component[] components = selectedGameObjects[i].GetComponents<Component>();

                gameObjectProperties.Add(new Dictionary<KeyValuePair<Component, string>, object>());

                foreach (Component component in components)
                {
                    if (component == null) continue;
                    SerializedObject serializedObject = new SerializedObject(component);
                    SerializedProperty iterator = serializedObject.GetIterator();
                    while (iterator.NextVisible(true))
                    {
                        object value = GetSerializedPropertyValue(iterator);
                        if (value == null) continue;
                        gameObjectProperties[i].TryAdd(new KeyValuePair<Component, string>(component, iterator.name),
                            GetSerializedPropertyValue(iterator));
                    }
                }
            }

            //Debug.Log("New Selection");
        }

        for (var index = 0; index < selectedGameObjects.Length; index++)
        {
            //Sync Property change
            Component[] components = selectedGameObjects[index].GetComponents<Component>();

            foreach (Component component in components)
            {
                if (component == null) continue;
                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty iterator = serializedObject.GetIterator();
                iterator.NextVisible(true);
                do
                {
                    object value = GetSerializedPropertyValue(iterator);
                    var key = new KeyValuePair<Component, string>(component, iterator.name);
                    if (value == null || !gameObjectProperties[index].ContainsKey(key) ||
                        gameObjectProperties[index][key].ToString() == value.ToString()) continue;
                    //Debug.Log("Updating Value of " + iterator.name + " to " + value + " was " + gameObjectProperties[index][new KeyValuePair<Component, string>(component, iterator.name)] + " on " + component.GetType().Name);
                    communication.SendPackage(new GameObjectSerializationPackage()
                    {
                        ComponentName = component.GetType().Name,
                        GameObjectHierarchy = GetPath(component.transform),
                        Value = value.ToString(),
                        VariableName = iterator.name
                    });
                    gameObjectProperties[index][new KeyValuePair<Component, string>(component, iterator.name)] = value;
                } while (iterator.NextVisible(false));
            }
        }
    }

    public static string GetPath(Transform current)
    {
        if (current.parent == null)
            return "/" + current.name;
        return GetPath(current.parent) + "/" + current.name;
    }

    private object GetSerializedPropertyValue(SerializedProperty iterator)
    {
        try
        {
            return iterator.propertyType switch
            {
                SerializedPropertyType.Vector3 => iterator.vector3Value,
                SerializedPropertyType.Quaternion => iterator.quaternionValue,
                SerializedPropertyType.Float => iterator.floatValue,
                SerializedPropertyType.Integer => iterator.intValue,
                SerializedPropertyType.Boolean => iterator.boolValue,
                SerializedPropertyType.String => iterator.stringValue,
                SerializedPropertyType.ObjectReference => iterator.objectReferenceInstanceIDValue,
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
                SerializedPropertyType.Gradient => null,
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
                iterator.intValue = int.Parse(value);
                break;
            case SerializedPropertyType.Boolean:
                iterator.boolValue = Convert.ToBoolean(value);
                break;
            case SerializedPropertyType.String:
                iterator.stringValue = value;
                break;
            case SerializedPropertyType.ObjectReference:
                //iterator.objectReferenceValue = (UnityEngine.Object) value;
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
        string[] sArray = sVector.Replace("(", "").Replace(")","").Split(',');
        
        return new Color(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));
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