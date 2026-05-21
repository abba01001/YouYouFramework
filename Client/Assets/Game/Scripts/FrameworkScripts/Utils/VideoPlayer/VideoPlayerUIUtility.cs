#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Video;

namespace GameScripts
{
    public class VideoPlayerUIUtility
    {
        [MenuItem("GameObject/UI/Video Player UI", false, 10)]
        private static void CreateVideoPlayerUI(MenuCommand menuCommand)
        {
            // 创建 Canvas
            GameObject canvas = GameObject.FindObjectOfType<Canvas>()?.gameObject;
            if (canvas == null)
            {
                canvas = new GameObject("Canvas", typeof(Canvas));
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<CanvasScaler>();
                canvas.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvas, "Create Canvas");
            }
    
            // 创建 RawImage
            GameObject rawImageGO = new GameObject("VideoPlayer_RawImage", typeof(RawImage));
            rawImageGO.transform.SetParent(canvas.transform, false);
    
            // 添加 AspectRatioFitter 自动适应
            AspectRatioFitter aspectFitter = rawImageGO.AddComponent<AspectRatioFitter>();
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
    
            // 添加 VideoPlayer
            VideoPlayer videoPlayer = rawImageGO.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
    
            // 创建一个默认 RenderTexture
            RenderTexture rt = new RenderTexture(1920, 1080, 0);
            videoPlayer.targetTexture = rt;
    
            // 赋值到 RawImage
            RawImage rawImage = rawImageGO.GetComponent<RawImage>();
            rawImage.texture = rt;
    
            // （可选）添加自定义封装控制脚本
            rawImageGO.AddComponent<VideoPlayerController>(); // 你自己的脚本类，自己改名字
    
            // 选中新建的对象
            Selection.activeGameObject = rawImageGO;
    
            Undo.RegisterCreatedObjectUndo(rawImageGO, "Create VideoPlayer UI");
        }
    }
}
#endif