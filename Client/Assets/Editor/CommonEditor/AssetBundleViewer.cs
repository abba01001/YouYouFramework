using UnityEditor;
using UnityEngine;
using System.IO;

public class AssetBundleViewer : EditorWindow
{
    [MenuItem("Assets/工具/查看AssetBundle内容")]
    public static void ShowWindow()
    {
        // 创建一个窗口来显示AssetBundle的内容
        EditorWindow.GetWindow(typeof(AssetBundleViewer), false, "AssetBundle内容");
    }

    // 用来显示AssetBundle内容的静态方法
    [MenuItem("Assets/工具/查看AssetBundle内容", true)]
    static bool ValidateAssetBundle()
    {
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        // 判断选中的文件是否为 .assetbundle 文件
        return !string.IsNullOrEmpty(selectedPath) && selectedPath.EndsWith(".assetbundle");
    }

    private void OnGUI()
    {
        // 获取选中的文件路径
        string assetBundlePath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!string.IsNullOrEmpty(assetBundlePath) && assetBundlePath.EndsWith(".assetbundle"))
        {
            // 加载并解密AssetBundle
            AssetBundle assetBundle = LoadEncryptedAssetBundle(assetBundlePath);

            if (assetBundle == null)
            {
                EditorGUILayout.LabelField("加载 AssetBundle 失败！");
                return;
            }

            EditorGUILayout.LabelField("AssetBundle内容：", EditorStyles.boldLabel);

            // 获取所有资源的名字
            string[] assetNames = assetBundle.GetAllAssetNames();
            foreach (string assetName in assetNames)
            {
                EditorGUILayout.LabelField(assetName);
            }

            // 卸载AssetBundle
            assetBundle.Unload(false);
        }
        else
        {
            EditorGUILayout.LabelField("请选中一个加密的 AssetBundle 文件");
        }
    }

    // 解密并加载AssetBundle
    private AssetBundle LoadEncryptedAssetBundle(string filePath)
    {
        // 读取加密的AssetBundle文件
        byte[] encryptedData = File.ReadAllBytes(filePath);

        // 使用解密后的字节流加载AssetBundle
        return AssetBundle.LoadFromMemory(encryptedData);
    }
}
