using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetPrinter : EditorWindow
{
    // 创建右键菜单项
    [MenuItem("Assets/Print All Asset Names In Folder", false, 100)]
    private static void PrintAllAssetNamesInFolder()
    {
        // 获取选中的文件夹路径
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        // 检查是否选中了文件夹
        if (Directory.Exists(selectedPath))
        {
            // 获取文件夹中的所有文件，排除 .meta 文件
            string[] assetPaths = Directory.GetFiles(selectedPath, "*", SearchOption.AllDirectories);
            
            // 用来存储所有文件名
            string result = "";

            // 打印所有文件名（去除扩展名）
            foreach (var assetPath in assetPaths)
            {
                // 排除 .meta 文件
                if (assetPath.EndsWith(".meta"))
                    continue;

                // 获取文件名并去除扩展名
                string assetName = Path.GetFileNameWithoutExtension(assetPath);

                // 添加到结果字符串，文件名之间以换行符分隔
                result += assetName + "\n";
            }

            // 打印所有文件名（换行分隔）
            Debug.Log(result);
        }
        else
        {
            Debug.LogError("请选择一个有效的文件夹！");
        }
    }
}