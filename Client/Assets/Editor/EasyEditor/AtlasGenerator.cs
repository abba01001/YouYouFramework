using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.U2D;
using UnityEngine.U2D;

public class AtlasGenerator : EditorWindow
{
    [MenuItem("Assets/工具/生成图集 %g")]
    static void GenerateAtlas()
    {
        // 获取当前选中的文件夹路径
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        // 确保选中的路径是文件夹
        if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
        {
            Debug.LogError("请选择一个文件夹！");
            return;
        }

        // 获取文件夹名称，用作Atlas名称
        string atlasName = Path.GetFileName(selectedPath);

        // 检查是否已有Atlas，若有则删除
        string atlasPath = selectedPath + "/" + atlasName + ".spriteatlas";
        if (File.Exists(atlasPath))
        {
            File.Delete(atlasPath);
            AssetDatabase.Refresh();
        }

        // 创建图集
        SpriteAtlas spriteAtlas = new SpriteAtlas();
        
        // 设置图集的打包选项（这里使用默认设置）
        SpriteAtlasPackingSettings packingSettings = spriteAtlas.GetPackingSettings();
        packingSettings.enableRotation = false;  // 允许旋转
        packingSettings.enableTightPacking = false;  // 紧凑打包
        spriteAtlas.SetPackingSettings(packingSettings);

        TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        platformSettings.name = "Android";  // 设置平台为 Android
        platformSettings.overridden = true;
        platformSettings.maxTextureSize = 2048;  // 设置最大纹理尺寸为 2048
        platformSettings.textureCompression = TextureImporterCompression.Compressed;  // 设置为压缩格式
        platformSettings.format = TextureImporterFormat.ASTC_6x6;  // 设置为 ASTC 6x6 压缩格式
        spriteAtlas.SetPlatformSettings(platformSettings); 
        AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);
        
        // 保存生成的Atlas
        AssetDatabase.CreateAsset(spriteAtlas, atlasPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 输出成功消息
        Debug.Log("图集生成成功！图集路径：" + atlasPath);
    }
}