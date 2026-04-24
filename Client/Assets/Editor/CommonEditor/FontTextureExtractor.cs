using UnityEditor;
using UnityEngine;
using System.IO;
using TMPro;

public class FontTextureExtractor : Editor
{
    // 右键菜单项：右键点击 FontAsset 文件时会显示 "Extract Texture"
    [MenuItem("Assets/工具/优化tmp字体纹理")]
    public static void ExtractTexture()
    {
        // 获取选中的字体文件
        Object selectedObj = Selection.activeObject;
        if (selectedObj != null && selectedObj is TMP_FontAsset)
        {
            TMP_FontAsset fontAsset = (TMP_FontAsset)selectedObj;
            string fontPath = AssetDatabase.GetAssetPath(fontAsset);

            // 执行提取纹理的操作
            ExtractTextureFromFont(fontPath);
        }
        else
        {
            Debug.LogError("Please select a TMP_FontAsset.");
        }
    }

    // 你的原有功能，未做改动
    public static void ExtractTextureFromFont(string fontPath)
    {
        string texturePath = fontPath.Replace(".asset", ".png");
        TMP_FontAsset targeFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath.Replace(Application.dataPath, "Assets"));
        
        Texture2D texture2D = new Texture2D(targeFontAsset.atlasTexture.width, targeFontAsset.atlasTexture.height, TextureFormat.Alpha8, false);
        Graphics.CopyTexture(targeFontAsset.atlasTexture, texture2D);
        
        byte[] dataBytes = texture2D.EncodeToPNG();
        FileStream fs = File.Open(texturePath, FileMode.OpenOrCreate);
        fs.Write(dataBytes, 0, dataBytes.Length);
        fs.Flush();
        fs.Close();
        
        // 刷新资产数据库
        AssetDatabase.Refresh();
        
        // 重新加载新生成的纹理，并更新字体资产
        Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath.Replace(Application.dataPath, "Assets"));
        AssetDatabase.RemoveObjectFromAsset(targeFontAsset.atlasTexture);
        targeFontAsset.atlasTextures[0] = atlas;
        targeFontAsset.material.mainTexture = atlas;
        
        // 保存修改
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Texture extracted and updated successfully!");
    }
}
