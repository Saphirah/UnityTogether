using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEngine;
[InitializeOnLoad]
public class ObjectChangeEventsExample
{
    static ObjectChangeEventsExample()
    {
        ObjectChangeEvents.changesPublished += ChangesPublished;
    }
    static void ChangesPublished(ref ObjectChangeEventStream stream)
    {
        for (int i = 0; i < stream.length; ++i)
        {
            switch (stream.GetEventType(i))
            {
                case ObjectChangeKind.ChangeScene:
                    stream.GetChangeSceneEvent(i, out var changeSceneEvent);
                    Debug.Log($"Change Scene Event: {changeSceneEvent.scene}");
                    break;
                case ObjectChangeKind.CreateGameObjectHierarchy:
                    stream.GetCreateGameObjectHierarchyEvent(i, out var createGameObjectHierarchyEvent);
                    GameObject newGameObject = EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId) as GameObject;
                    Communication.Instance?.SendPackage(new GameObjectCreatePackage()
                    { 
                        NewGameObject = JsonSerialization.ToJson(newGameObject),
                        NewParentHierarchy = newGameObject.transform.parent.gameObject.GetPath()
                    });
                    Debug.Log($"Create GameObject: {newGameObject} in scene {createGameObjectHierarchyEvent.scene}.");
                    break;
                case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
                    stream.GetChangeGameObjectStructureHierarchyEvent(i, out var changeGameObjectStructureHierarchy);
                    var gameObject = EditorUtility.InstanceIDToObject(changeGameObjectStructureHierarchy.instanceId) as GameObject;
                    Debug.Log($"Change GameObject hierarchy: {gameObject} in scene {changeGameObjectStructureHierarchy.scene}.");
                    break;
                case ObjectChangeKind.ChangeGameObjectStructure:
                    stream.GetChangeGameObjectStructureEvent(i, out var changeGameObjectStructure);
                    var gameObjectStructure = EditorUtility.InstanceIDToObject(changeGameObjectStructure.instanceId) as GameObject;
                    Debug.Log($"Change GameObject structure: {gameObjectStructure} in scene {changeGameObjectStructure.scene}.");
                    break;
                //Implemented
                case ObjectChangeKind.ChangeGameObjectParent:
                    stream.GetChangeGameObjectParentEvent(i, out var changeGameObjectParent);
                    var gameObjectChanged = EditorUtility.InstanceIDToObject(changeGameObjectParent.instanceId) as GameObject;
                    var newParentGo = EditorUtility.InstanceIDToObject(changeGameObjectParent.newParentInstanceId) as GameObject;
                    var oldParentGo = EditorUtility.InstanceIDToObject(changeGameObjectParent.previousParentInstanceId) as GameObject;
                    Debug.Log((oldParentGo ? oldParentGo.GetPath() : "") + "/" + gameObjectChanged.name);
                    Communication.Instance?.SendPackage(new GameObjectChangeParentPackage()
                    {
                        GameObjectHierarchy = (oldParentGo ? oldParentGo.GetPath() : "") + "/" + gameObjectChanged.name,
                        NewParentHierarchy = newParentGo ? newParentGo.GetPath() : "/"
                    });
                    break;
                case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
                    stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var changeGameObjectOrComponent);
                    var goOrComponent = EditorUtility.InstanceIDToObject(changeGameObjectOrComponent.instanceId);
                    if (goOrComponent is GameObject go)
                    {   
                        //Debug.Log($"GameObject {go} change properties in scene {changeGameObjectOrComponent.scene}.");
                    }
                    else if (goOrComponent is Component component)
                    {
                        //Debug.Log($"Component {component} change properties in scene {changeGameObjectOrComponent.scene}.");
                    }
                    break;
                //Implemented
                case ObjectChangeKind.DestroyGameObjectHierarchy:
                    stream.GetDestroyGameObjectHierarchyEvent(i, out var destroyGameObjectHierarchyEvent);
                    var destroyGo = EditorUtility.InstanceIDToObject(destroyGameObjectHierarchyEvent.parentInstanceId) as GameObject;
                    Debug.Log($"Destroy GameObject hierarchy. GameObject: {destroyGo} in scene {destroyGameObjectHierarchyEvent.scene}.");
                    Communication.Instance?.SendPackage(new GameObjectDestroyPackage()
                    { 
                        GameObjectHierarchy = destroyGo.GetPath() + "/" + destroyGo.name,
                    });
                    break;
                case ObjectChangeKind.CreateAssetObject:
                    stream.GetCreateAssetObjectEvent(i, out var createAssetObjectEvent);
                    var createdAsset = EditorUtility.InstanceIDToObject(createAssetObjectEvent.instanceId);
                    var createdAssetPath = AssetDatabase.GUIDToAssetPath(createAssetObjectEvent.guid);
                    Debug.Log($"Created asset {createdAsset} at {createdAssetPath} in scene {createAssetObjectEvent.scene}.");
                    break;
                case ObjectChangeKind.DestroyAssetObject:
                    stream.GetDestroyAssetObjectEvent(i, out var destroyAssetObjectEvent);
                    var destroyAsset = EditorUtility.InstanceIDToObject(destroyAssetObjectEvent.instanceId);
                    var destroyAssetPath = AssetDatabase.GUIDToAssetPath(destroyAssetObjectEvent.guid);
                    Debug.Log($"Destroy asset {destroyAsset} at {destroyAssetPath} in scene {destroyAssetObjectEvent.scene}.");
                    break;
                case ObjectChangeKind.ChangeAssetObjectProperties:
                    stream.GetChangeAssetObjectPropertiesEvent(i, out var changeAssetObjectPropertiesEvent);
                    var changeAsset = EditorUtility.InstanceIDToObject(changeAssetObjectPropertiesEvent.instanceId);
                    var changeAssetPath = AssetDatabase.GUIDToAssetPath(changeAssetObjectPropertiesEvent.guid);
                    Debug.Log($"Change asset {changeAsset} at {changeAssetPath} in scene {changeAssetObjectPropertiesEvent.scene}.");
                    break;
                case ObjectChangeKind.UpdatePrefabInstances:
                    stream.GetUpdatePrefabInstancesEvent(i, out var updatePrefabInstancesEvent);
                    var ss = new StringBuilder();
                    ss.AppendLine($"Update Prefabs in scene {updatePrefabInstancesEvent.scene}");
                    foreach (var prefabId in updatePrefabInstancesEvent.instanceIds)
                    {
                        ss.AppendLine(EditorUtility.InstanceIDToObject(prefabId).ToString());
                    }
                    Debug.Log(ss.ToString());
                    break;
            }
        }
    }
}
 