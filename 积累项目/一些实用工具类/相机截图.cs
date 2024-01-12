using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class ScreenshotTool
{
    static string screen_shot_path = Application.dataPath + "/Screenshot/";

    [MenuItem("GameObject/Screenshot", false, -1)]
    static void Screenshot()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj == null) return;
        Camera cam = obj.GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("请选择相机截图");
            return;
        }
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = new Color(1, 1, 1, 0);
        Vector2 size = GetMainGameViewSize();
        GeneratorTexture(cam, new Rect(0, 0, size.x, size.y));
        AssetDatabase.Refresh();
    }

    static void GeneratorTexture(Camera camera, Rect rect)
    {

        RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 24);
        camera.targetTexture = rt;
        camera.Render();
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null; 
        GameObject.DestroyImmediate(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        string str =  Convert.ToInt64(ts.TotalSeconds).ToString();
        if (!File.Exists(screen_shot_path))
        {
            Directory.CreateDirectory(screen_shot_path);
        }
        string filename = screen_shot_path + str + ".png";
        File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("截屏了一张照片: {0}", filename));
    }

    static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object size = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)size;
    }

}
