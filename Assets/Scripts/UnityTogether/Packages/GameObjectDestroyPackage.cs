using UnityEngine;
using Object = UnityEngine.Object;

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

    public override void Execute() => Object.DestroyImmediate(GameObject);
}
