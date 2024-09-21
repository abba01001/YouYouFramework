using System;
using Main;
using UnityEditor;
using UnityEngine;

[Flags]
public enum PlatformOption
{
    Windows,
    Android
}

[CreateAssetMenu(fileName = "CosConfig", menuName = "ScriptableObjects/CosConfig", order = 1)]
public class CosConfig : ScriptableObject
{
    public string secretId = "";
    public string secretKey = "";
    public string region = "";
    public string bucket = "";
    public string cosVersionPath = "";

    public PlatformOption platformOption
    {
        get
        {
            BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            switch (activeBuildTarget)
            {
                case BuildTarget.Android:
                    return PlatformOption.Android;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return PlatformOption.Windows;
                default:
                    return PlatformOption.Windows;  // 默认返回Windows
            }
        }
        set{}
    } // 使用枚举类型表示平台选项

    // 添加一个属性，根据平台选项返回相应的路径
    public string cosABRoot
    {
        get
        {
            switch (platformOption)
            {
                case PlatformOption.Windows:
                    return "/Unity/AssetBundle/"+ PlayerPrefs.GetString(YFConstDefine.AssetVersion)+"/"+ "Windows"+ "/";
                case PlatformOption.Android:
                    return "/Unity/AssetBundle/" + PlayerPrefs.GetString(YFConstDefine.AssetVersion)+"/" + "Android" + "/";
                // 添加其他平台选项
                default:
                    return "";
            }
        }
    }
    public string appid = "";
}