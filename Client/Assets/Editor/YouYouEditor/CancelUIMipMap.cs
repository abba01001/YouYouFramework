using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public class CancleUIMipMap
{

    [MenuItem("YouYouTools/取消贴图MipMap和读写")]
    static void GetTexture()
    {
        List<string> allTexturePaths = new List<string>();
        string textureType = "*.jpg,*.png,*.bmp";
        //得到所有图片格式
        string[] textureTypeArray = textureType.Split(',');

        for (int i = 0; i < textureTypeArray.Length; i++)
        {
            //string[] texturePath = Directory.GetFiles(Application.dataPath,"(*.jpg|*.bmp)");
            string[] texturePath = Directory.GetFiles("Assets/", textureTypeArray[i], SearchOption.AllDirectories);
            for (int j = 0; j < texturePath.Length; j++)
            {
                allTexturePaths.Add(texturePath[j]);
                //Debug.Log(texturePath[j]);
            }
        }

        for (int k = 0; k < allTexturePaths.Count; k++)
        {
            Texture2D tx = new Texture2D(200, 200);
            tx.LoadImage(getTextureByte(allTexturePaths[k]));
            //如果图片不符合规        范
            if (!(isPower(tx.height) && isPower(tx.width)))
            {
                Debug.Log("不符合规范的图片的尺寸为:" + tx.width + "X" + tx.height);
            }
            TextureImporter textureImporter = TextureImporter.GetAtPath(allTexturePaths[k]) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = false;
            textureImporter.SaveAndReimport();
            AssetDatabase.ImportAsset(allTexturePaths[k]);
        }


    }
    /// <summary>
    /// 根据图片路径返回字节流
    /// </summary>
    /// <param name="texturePath"></param>
    /// <returns></returns>
    static byte[] getTextureByte(string texturePath)
    {
        FileStream file = new FileStream(texturePath, FileMode.Open);
        byte[] txByte = new byte[file.Length];
        file.Read(txByte, 0, txByte.Length);
        file.Close();
        return txByte;
    }
    /// <summary>
    /// 判断图片尺寸是否为2的n次方
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    static bool isPower(int n)
    {
        if (n < 1)
            return false;
        int i = 1;
        while (i <= n)
        {
            if (i == n)
                return true;
            i <<= 1;

        }
        return false;
    }

}