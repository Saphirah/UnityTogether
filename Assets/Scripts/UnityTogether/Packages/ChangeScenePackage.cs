using System;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ChangeScenePackage : Package
{
    public ChangeScenePackage(){}
    public ChangeScenePackage(string json) => Deserialize(json);

    public string SceneName;
    public override void Execute()
    {
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(SceneName);
    }
}
