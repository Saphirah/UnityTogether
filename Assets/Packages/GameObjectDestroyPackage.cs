using System;
using UnityEngine;

public class GameObjectDestroyPackage : Package
{
    public GameObjectDestroyPackage(){}
    public GameObjectDestroyPackage(string json) => Deserialize(json);

    public string GameObjectHierarchy;

    public GameObject GameObject
    {
        get
        {
            Debug.Log("GameObjectHierarchy: " + GameObjectHierarchy);
            return GameObject.Find(GameObjectHierarchy);
        }
    }
}
