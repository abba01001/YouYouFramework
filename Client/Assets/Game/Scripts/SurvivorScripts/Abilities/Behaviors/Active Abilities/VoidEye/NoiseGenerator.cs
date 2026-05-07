using UnityEngine;
using System.IO;

namespace GameScripts
{
    public class NoiseGenerator : MonoBehaviour
    {
        public int size = 256;
        public float scale = 20f;

        [ContextMenu("Generate Noise")]
        public void Generate()
        {
            Texture2D tex = new Texture2D(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 计算柏林噪声
                    float xCoord = (float)x / size * scale;
                    float yCoord = (float)y / size * scale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    tex.SetPixel(x, y, new Color(sample, sample, sample));
                }
            }
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/LaserNoise.png", bytes);
            Debug.Log("噪声贴图已生成到 Assets 文件夹！");
        }
    }
}