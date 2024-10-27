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
    public string cosVersionPath = "";

    public PlatformOption platformOption
    {
        get
        {
#if TestMode
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
#else
            return PlatformOption.Android;
#endif
        }
        set{}
    } // 使用枚举类型表示平台选项

    public string appid = "";
}