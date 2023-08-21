using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class GameObjectDictionary
{
    public static Dictionary<int, string> gameObjectPaths = new();
    public static Dictionary<GameObject, Component[]> components = new();

    [InitializeOnLoadMethod]
    static void OnEditorLoad()
    {
        ObjectChangeEvents.changesPublished += ChangesPublished;
        Selection.selectionChanged += OnSelectionChanged;
        OnSelectionChanged();
        //CreatePaths();
    }

    /**public static void CreatePaths()
    {
        Debug.Log("Creating paths");
        GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.scene.name == null) continue;
            gameObjectPaths.Add(gameObject.GetInstanceID(), gameObject.GetPath());
        }
    }*/

    public static void OnSelectionChanged()
    {
        components.Clear();
        foreach (GameObject gameObject in Selection.gameObjects)
        {
            gameObjectPaths[gameObject.GetInstanceID()] = gameObject.GetPath();
            components[gameObject] = gameObject.GetComponents<Component>();
        }
    }
    
    public static string GetPath(int id) => gameObjectPaths[id];
    public static Component[] GetComponents(GameObject gameObject) => components[gameObject];
    
    // Call this method whenever a GameObject is created or its hierarchy changes
    public static void ChangesPublished(ref ObjectChangeEventStream stream)
    {
        for (int i = 0; i < stream.length; ++i)
        {
            GameObject gameObject;
            switch (stream.GetEventType(i))
            {
                case ObjectChangeKind.CreateGameObjectHierarchy:
                    stream.GetCreateGameObjectHierarchyEvent(i, out var changeGameObjectStructureHierarchy);
                    gameObject =
                        EditorUtility.InstanceIDToObject(changeGameObjectStructureHierarchy.instanceId) as GameObject;
                    gameObjectPaths[changeGameObjectStructureHierarchy.instanceId] = gameObject.GetPath();
                    Debug.Log("Created: " + gameObject.GetPath());
                    break;
                case ObjectChangeKind.ChangeGameObjectParent:
                    stream.GetChangeGameObjectParentEvent(i, out var changeGameObjectParent);
                    gameObject = EditorUtility.InstanceIDToObject(changeGameObjectParent.instanceId) as GameObject;
                    gameObjectPaths[changeGameObjectParent.instanceId] = gameObject.GetPath();
                    Debug.Log("Changed: " + gameObject.GetPath());
                    break;
                case ObjectChangeKind.ChangeGameObjectStructure:
                    OnSelectionChanged();
                    break;
            }
        }
    }
}