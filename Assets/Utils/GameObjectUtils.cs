using UnityEngine;

public static class GameObjectUtils
{
    public static string GetPath(this GameObject current)
    {
        if(current == null) return "/";
        if (current.transform.parent == null)
            return "/" + current.name;
        return GetPath(current.transform.parent.gameObject) + "/" + current.name;
    }
}