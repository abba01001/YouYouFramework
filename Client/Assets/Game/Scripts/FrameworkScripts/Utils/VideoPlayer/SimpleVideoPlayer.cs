using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


namespace FrameWork
{
    public class SimpleVideoPlayer : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        public RawImage rawImage;
        public VideoPlayerController videoController;
        public VideoClip videoClip;

        void Start()
        {
            // 创建RenderTexture
            RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);
            videoPlayer.targetTexture = renderTexture;
            rawImage.texture = renderTexture;

            videoController.OnPlayStarted += () => Debug.LogError("播放开始");
            videoController.OnPlayCompleted += () => Debug.LogError("播放完成");
            videoController.OnError += (error) => Debug.LogError($"播放错误: {error}");
            videoController.SetVideoClip(videoClip);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                videoController.Play();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                videoController.Pause();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                videoController.SetProgress(0.2f);
            }
        }
    }
}