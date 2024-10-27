using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CosConfig))]
public class CosConfigInspector : Editor
{

    private void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 获取当前的构建平台
        BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

        // 根据构建平台设置 cosABRoot 字段
        CosConfig targetCosConfig = (CosConfig)target;

        serializedObject.ApplyModifiedProperties();
    }

    // 根据构建平台获取对应的 PlatformOption 值
    private PlatformOption GetPlatformOption(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return PlatformOption.Windows;
            case BuildTarget.Android:
                return PlatformOption.Android;
            // 添加其他平台的映射
            default:
                return PlatformOption.Windows; // 默认返回 Windows
        }
    }
}
