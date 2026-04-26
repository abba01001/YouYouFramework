using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SpriteExtractor
{
    [MenuItem("Assets/提取图集", false, 100)]
    public static void ExtractSprites()
    {
        Object[] selectedObjects = Selection.objects;
        
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogError("未选中任何资源！");
            return;
        }

        // 过滤出有效的贴图
        List<Texture2D> targetTextures = selectedObjects
            .Where(obj => obj is Texture2D)
            .Cast<Texture2D>()
            .ToList();

        if (targetTextures.Count == 0) return;

        for (int i = 0; i < targetTextures.Count; i++)
        {
            Texture2D texture = targetTextures[i];
            string path = AssetDatabase.GetAssetPath(texture);
            
            // 更新进度条 (第一层：总体文件进度)
            float totalProgress = (float)i / targetTextures.Count;
            EditorUtility.DisplayProgressBar("提取 Sprite 中...", $"正在处理图集 ({i + 1}/{targetTextures.Count}): {texture.name}", totalProgress);

            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;

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
                string dirPath = Path.Combine(Path.GetDirectoryName(path), texture.name);
                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                // 筛选出 Sprite
                List<Sprite> sprites = assets.OfType<Sprite>().ToList();
                
                for (int j = 0; j < sprites.Count; j++)
                {
                    Sprite sprite = sprites[j];
                    
                    // 更新进度条 (第二层：单个图集内的进度)
                    float subProgress = (float)j / sprites.Count;
                    EditorUtility.DisplayProgressBar("提取 Sprite 中...", $"[{texture.name}] 正在导出: {sprite.name} ({j + 1}/{sprites.Count})", totalProgress + (subProgress / targetTextures.Count));

                    string savePath = Path.Combine(dirPath, sprite.name + ".png");
                    SaveSpriteAsPng(sprite, savePath);
                    SetSpriteImportSettings(savePath);
                }
                Debug.Log($"{texture.name}：成功提取 {sprites.Count} 张 Sprites");
            }
            finally
            {
                ti.isReadable = oldReadable;
                ti.textureCompression = oldCompression;
                ti.SaveAndReimport();
            }
        }

        // 必须执行，否则进度条会卡死在屏幕上
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    private static void SaveSpriteAsPng(Sprite sprite, string savePath)
    {
        Texture2D tex = sprite.texture;
        Rect r = sprite.textureRect;
        Texture2D newTex = new Texture2D((int)r.width, (int)r.height, TextureFormat.RGBA32, false);
        Color[] pixels = tex.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
        newTex.SetPixels(pixels);
        newTex.Apply();
        byte[] bytes = newTex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        Object.DestroyImmediate(newTex);
    }

    private static void SetSpriteImportSettings(string assetPath)
    {
        string unityPath = assetPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
        if (!unityPath.StartsWith("Assets"))
        {
            int index = assetPath.IndexOf("Assets");
            if (index != -1) unityPath = assetPath.Substring(index).Replace("\\", "/");
        }

        AssetDatabase.ImportAsset(unityPath, ImportAssetOptions.ForceUpdate);
        TextureImporter ti = AssetImporter.GetAtPath(unityPath) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
        }
    }
}