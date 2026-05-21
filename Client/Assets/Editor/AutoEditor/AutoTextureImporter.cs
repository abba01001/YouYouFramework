using UnityEngine;
using UnityEditor;

public class AutoTextureImporter : AssetPostprocessor
{
    private static readonly string modelTexturePath = "Assets/Game/Download/Textures/Model";
    private static readonly string bgTexturePath = "Assets/Game/Download/Textures/BackGround";

    void OnPostprocessTexture(Texture2D texture)
    {
        // 只处理指定文件夹中的图片
        if (!assetPath.StartsWith(modelTexturePath) && !assetPath.StartsWith(bgTexturePath)) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        if (importer != null)
        {
            bool settingsChanged = false;
            // 设置纹理类型为 Sprite
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                settingsChanged = true;
            }

            // 配置平台特定设置
            InitSetting(importer, ref settingsChanged);

            // 如果设置有更改，则执行导入操作
            if (settingsChanged)
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }

    private void InitSetting(TextureImporter importer, ref bool settingsChanged)
    {
        TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Android");

        if (assetPath.StartsWith(modelTexturePath))
        {
            if (platformSettings.format != TextureImporterFormat.ASTC_4x4)
            {
                platformSettings.format = TextureImporterFormat.ASTC_4x4;
                platformSettings.overridden = true;
                importer.SetPlatformTextureSettings(platformSettings);
                settingsChanged = true;
            }
        }
        else if (assetPath.StartsWith(bgTexturePath))
        {
            if (platformSettings.format != TextureImporterFormat.ASTC_6x6)
            {
                platformSettings.format = TextureImporterFormat.ASTC_6x6;
                platformSettings.overridden = true;
                importer.SetPlatformTextureSettings(platformSettings);
                settingsChanged = true;
            }
        }
    }
}
