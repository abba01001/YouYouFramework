using UnityEditor;
using UnityEngine;

public class TextureOptimizer : Editor
{
    // 在工具菜单下创建优化纹理功能
    [MenuItem("Assets/工具/优化纹理")]
    public static void OptimizeTexturesInSelectedFolder()
    {
        // 获取当前选中的对象
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogError("No objects selected.");
            return;
        }

        // 获取文件夹路径
        string folderPath = GetFolderPathFromSelection(selectedObjects[0]);

        // 调试输出路径，确认是否正确
        Debug.Log("Selected path: " + folderPath);

        // 确保路径不为空且是文件夹
        if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Please select a valid folder.");
            return;
        }

        // 调用优化方法
        OptimizeTexturesInFolder(folderPath);
    }

    private static string GetFolderPathFromSelection(Object selectedObject)
    {
        // 如果选择的是文件夹，则返回文件夹路径
        string folderPath = AssetDatabase.GetAssetPath(selectedObject);
        
        // 如果选择的是文件夹的外观（比如右侧窗口中的文件夹），路径应该有效
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return folderPath;
        }

        // 如果没有正确选择文件夹，再尝试从左侧选择的内容中获取路径
        return null;
    }

    private static void OptimizeTexturesInFolder(string folderPath)
    {
        // 获取指定文件夹下的所有纹理资源
        string[] assetPaths = AssetDatabase.FindAssets("t:texture", new[] { folderPath });

        // 如果没有找到纹理资源，输出信息
        if (assetPaths.Length == 0)
        {
            Debug.LogWarning("No textures found in the selected folder.");
        }

        foreach (string assetPath in assetPaths)
        {
            // 获取每个纹理的路径
            string path = AssetDatabase.GUIDToAssetPath(assetPath);

            // 加载纹理
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (texture != null)
            {
                // 调用优化方法
                OptimizeTexture(path);
            }
            else
            {
                Debug.LogWarning("Not a valid texture: " + path);
            }
        }

        // 刷新 AssetDatabase，确保更改被应用
        AssetDatabase.Refresh();
        Debug.Log("Texture optimization completed.");
    }

    private static void OptimizeTexture(string path)
    {
        // 获取纹理的导入器
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null)
        {
            // 设置为 ASTC 4x4 格式
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = true;

            // 修改平台设置
            TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings
            {
                name = "Android", // 确保修改的是 Android 平台
                overridden = true,
                format = TextureImporterFormat.ASTC_4x4
            };

            // 设置平台配置
            importer.SetPlatformTextureSettings(platformSettings);

            // 重新导入纹理，应用设置
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log("Texture optimized: " + path);
        }
        else
        {
            Debug.LogWarning("Not a valid texture: " + path);
        }
    }
}
