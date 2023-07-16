using System;
using UnityEngine;

public class ChangeScenePackage : Package
{
    public ChangeScenePackage(){}
    public ChangeScenePackage(string json) => Deserialize(json);

    public string SceneName;
}
