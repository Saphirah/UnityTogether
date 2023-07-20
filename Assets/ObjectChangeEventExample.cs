using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad]
public class ObjectChangeEventsExample
{
    static ObjectChangeEventsExample()
    {
        ObjectChangeEvents.changesPublished += ChangesPublished;
    }

    private static bool isActive = true;
    public static void SetActive(bool active) => isActive = active;

    static void ChangesPublished(ref ObjectChangeEventStream stream)
    {
        if (!isActive) return;
        for (int i = 0; i < stream.length; ++i)
        {
            switch (stream.GetEventType(i))
            {
                case ObjectChangeKind.ChangeScene:
                    stream.GetChangeSceneEvent(i, out var changeSceneEvent);
                    Debug.Log($"Change Scene Event: {changeSceneEvent.scene}");
                    UnityTogetherClient.Instance?.SendPackage(new ChangeScenePackage() { SceneName = changeSceneEvent.scene.path });
                    break;
                case ObjectChangeKind.CreateGameObjectHierarchy:
                    stream.GetCreateGameObjectHierarchyEvent(i, out var createGameObjectHierarchyEvent);
                    GameObject newGameObject = EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId) as GameObject;
                    UnityTogetherClient.Instance?.SendPackage(new GameObjectCreatePackage()
                    { 
                        GameObjectHierarchy = newGameObject.transform.gameObject.GetPath(),
                        GameObjectName = newGameObject.name
                    });
                    foreach (Component component in newGameObject.GetComponents<Component>())
                    {
                        if(component is not Transform)
                            UnityTogetherClient.Instance?.SendPackage(new GameObjectAddComponentPackage()
                            {
                                GameObjectHierarchy = newGameObject.transform.gameObject.GetPath(),
                                ComponentName = component.GetType().AssemblyQualifiedName
                            });
                        SerializedObject serializedObject = new SerializedObject(component);
                        SerializedProperty property = serializedObject.GetIterator();
                        while (property.Next(true))
                        {
                            UnityTogetherClient.Instance?.SendPackage(new GameObjectSerializationPackage()
                            {
                                GameObjectHierarchy = newGameObject.GetPath(),
                                ComponentName = component?.GetType().AssemblyQualifiedName,
                                ComponentIndex = newGameObject.GetComponents<Component>().ToList().IndexOf(component),
                                VariableName = property?.name,
                                Value = GameObjectSync.GetSerializedPropertyValue(property)?.ToString()
                            });
                        }
                    }
                    Debug.Log(newGameObject.GetPath());
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
                    Component[] componentStructure = gameObjectStructure.GetComponents<Component>();
                    Component[] componentStructureOld = GameObjectDictionary.GetComponents(gameObjectStructure);

                    foreach (Component component in componentStructureOld)
                    {
                        if(componentStructure.Contains(component)) continue;
                        UnityTogetherClient.Instance?.SendPackage(new GameObjectRemoveComponentPackage()
                        {
                            GameObjectHierarchy = gameObjectStructure.transform.gameObject.GetPath(),
                            ComponentName = component.GetType().AssemblyQualifiedName,
                            ComponentIndex = componentStructureOld.Where(comp => comp.GetType() == component.GetType()).ToList().IndexOf(component)
                        });
                    }
                    
                    foreach (Component component in componentStructure)
                    {
                        if(componentStructureOld.Contains(component)) continue;
                        UnityTogetherClient.Instance?.SendPackage(new GameObjectAddComponentPackage()
                        {
                            GameObjectHierarchy = gameObjectStructure.transform.gameObject.GetPath(),
                            ComponentName = component.GetType().AssemblyQualifiedName
                        });
                    }

                    Debug.Log($"Change GameObject structure: {gameObjectStructure} in scene {changeGameObjectStructure.scene}.");
                    break;
                case ObjectChangeKind.ChangeGameObjectParent:
                    stream.GetChangeGameObjectParentEvent(i, out var changeGameObjectParent);
                    var gameObjectChanged = EditorUtility.InstanceIDToObject(changeGameObjectParent.instanceId) as GameObject;
                    var newParentGo = EditorUtility.InstanceIDToObject(changeGameObjectParent.newParentInstanceId) as GameObject;
                    var oldParentGo = EditorUtility.InstanceIDToObject(changeGameObjectParent.previousParentInstanceId) as GameObject;
                    Debug.Log((oldParentGo ? oldParentGo.GetPath() : "") + "/" + gameObjectChanged.name);
                    UnityTogetherClient.Instance?.SendPackage(new GameObjectChangeParentPackage()
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
                        Debug.Log($"GameObject {go} change properties in scene {changeGameObjectOrComponent.scene}.");
                    }
                    else if (goOrComponent is Component component)
                    {
                        Debug.Log($"Component {component} change properties in scene {changeGameObjectOrComponent.scene}.");
                    }
                    break;
                case ObjectChangeKind.DestroyGameObjectHierarchy:
                    stream.GetDestroyGameObjectHierarchyEvent(i, out var destroyGameObjectHierarchyEvent);
                    string path = GameObjectDictionary.GetPath(destroyGameObjectHierarchyEvent.instanceId);
                    Debug.Log($"Destroy GameObject hierarchy. GameObject: {path} in scene {destroyGameObjectHierarchyEvent.scene}.");
                    UnityTogetherClient.Instance?.SendPackage(new GameObjectDestroyPackage()
                    { 
                        GameObjectHierarchy = path,
                    });
                    break;
                case ObjectChangeKind.ChangeAssetObjectProperties:
                    stream.GetChangeAssetObjectPropertiesEvent(i, out var changeAssetObjectPropertiesEvent);
                    var changeAsset = EditorUtility.InstanceIDToObject(changeAssetObjectPropertiesEvent.instanceId);
                    var changeAssetPath = AssetDatabase.GUIDToAssetPath(changeAssetObjectPropertiesEvent.guid);
                    Debug.Log($"Change asset {changeAsset} at {changeAssetPath} in scene {changeAssetObjectPropertiesEvent.scene}.");
                    break;
            }
        }
    }
}
 