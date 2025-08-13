//@LeeTools
//------------------------
//Filename：FrameworkScriptableObject.cs
//Author：Admin
//Device：NORAHCATHY
//Email：53033907+leexyy0804@users.noreply.github.com
//CreateDate：2025/08/13 23:30:01
//Function：Nothing
//------------------------

using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "FrameworkConfig", menuName = "LeeFramework/Configs/FrameworkConfig")]
public class FrameworkConfig : ScriptableObject
{
    public SceneDataNode[] sceneConfigs;
}

[Serializable]
public class SceneDataNode
{
    public string scenePath;
    public string sceneName;
    public string Name => sceneName;
    public GameObject scene;
}
