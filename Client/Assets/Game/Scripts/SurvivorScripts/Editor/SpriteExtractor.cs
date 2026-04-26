using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteExtractor
{
    [MenuItem("Assets/Extract Sprites to Folder", false, 100)]
    public static void ExtractSprites()
    {
        Object selected = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(selected);
        Texture2D texture = selected as Texture2D;

        if (texture == null)
        {
            Debug.LogError("请选择一张 Texture 图片！");
            return;
        }

        // 1. 自动修改 Import Settings 确保可读
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;

        bool oldReadable = ti.isReadable;
        TextureImporterCompression oldCompression = ti.textureCompression;

        if (!oldReadable || oldCompression != TextureImporterCompression.Uncompressed)
        {
            ti.isReadable = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }

        try
        {
            // 2. 创建文件夹
            string dirPath = Path.Combine(Path.GetDirectoryName(path), texture.name);
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            // 3. 提取所有 Sprite
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            int count = 0;
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    SaveSpriteAsPng(sprite, Path.Combine(dirPath, sprite.name + ".png"));
                    count++;
                }
            }
            Debug.Log($"成功提取 {count} 张 Sprites 到: {dirPath}");
        }
        finally
        {
            // 4. 还原设置，避免占用内存或影响包体大小
            ti.isReadable = oldReadable;
            ti.textureCompression = oldCompression;
            ti.SaveAndReimport();
            AssetDatabase.Refresh();
        }
    }

    private static void SaveSpriteAsPng(Sprite sprite, string savePath)
    {
        Texture2D tex = sprite.texture;
        Rect r = sprite.textureRect;
        
        // 使用该构造函数可以支持透明度
        Texture2D newTex = new Texture2D((int)r.width, (int)r.height, TextureFormat.RGBA32, false);
        
        // 核心：从原图获取指定区域的像素
        Color[] pixels = tex.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
        
        newTex.SetPixels(pixels);
        newTex.Apply();

        byte[] bytes = newTex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        Object.DestroyImmediate(newTex);
    }
}