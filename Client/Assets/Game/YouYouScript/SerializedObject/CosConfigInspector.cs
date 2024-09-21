using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CosConfig))]
public class CosConfigInspector : Editor
{
    SerializedProperty secretIdProp;
    SerializedProperty secretKeyProp;
    SerializedProperty regionProp;
    SerializedProperty bucketProp;
    SerializedProperty appidProp;

    private void OnEnable()
    {
        secretIdProp = serializedObject.FindProperty("secretId");
        secretKeyProp = serializedObject.FindProperty("secretKey");
        regionProp = serializedObject.FindProperty("region");
        bucketProp = serializedObject.FindProperty("bucket");
        appidProp = serializedObject.FindProperty("appid");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(secretIdProp);
        EditorGUILayout.PropertyField(secretKeyProp);
        EditorGUILayout.PropertyField(regionProp);
        EditorGUILayout.PropertyField(bucketProp);

        // 获取当前的构建平台
        BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

        // 根据构建平台设置 cosABRoot 字段
        CosConfig targetCosConfig = (CosConfig)target;
        targetCosConfig.platformOption = GetPlatformOption(activeBuildTarget);
        EditorUtility.SetDirty(targetCosConfig);

        EditorGUILayout.LabelField("cosABRoot", targetCosConfig.cosABRoot);

        EditorGUILayout.PropertyField(appidProp);
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
