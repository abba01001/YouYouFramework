using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public interface IVideoPlayerController
{
    void Play();
    void Pause();
    void Stop();

    void SetProgress(float progress);
    float GetProgress();

    void SetVolume(float volume);
    float GetVolume();

    bool IsPlaying { get; }
    bool IsPaused { get; }
    float Duration { get; }

    event Action OnPlayStarted;
    event Action OnPlayPaused;
    event Action OnPlayStopped;
    event Action OnPlayCompleted;
    event Action<string> OnError;
}

[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayerController : MonoBehaviour, IVideoPlayerController
{
    private VideoPlayer _videoPlayer;
    private bool _isInitialized = false;
    private float _targetProgress = -1f;
    private Coroutine _progressUpdateCoroutine;

    [Header("播放器配置")]
    [SerializeField] private bool autoPlay = false;
    [SerializeField] private bool loop = false;
    [SerializeField] private float defaultVolume = 1.0f;

    [Header("性能优化")]
    [SerializeField] private bool skipOnDrop = true;
    [SerializeField] private VideoAspectRatio aspectRatio = VideoAspectRatio.Stretch;

    public bool IsPlaying => _videoPlayer != null && _videoPlayer.isPlaying;
    public bool IsPaused => _videoPlayer != null && _videoPlayer.isPaused;
    public float Duration => _videoPlayer != null ? (float)_videoPlayer.length : 0f;

    public event Action OnPlayStarted;
    public event Action OnPlayPaused;
    public event Action OnPlayStopped;
    public event Action OnPlayCompleted;
    public event Action<string> OnError;

    private void Awake()
    {
        InitializeVideoPlayer();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
        StopProgressCoroutine();
    }

    /// <summary>
    /// 初始化视频播放器
    /// </summary>
    private void InitializeVideoPlayer()
    {
        try
        {
            _videoPlayer = GetComponent<VideoPlayer>();
            ConfigureVideoPlayer();
            SubscribeEvents();
            _isInitialized = true;

            if (autoPlay)
            {
                Play();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"VideoPlayer初始化失败: {ex.Message}");
            OnError?.Invoke($"初始化失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 配置视频播放器参数
    /// </summary>
    private void ConfigureVideoPlayer()
    {
        _videoPlayer.playOnAwake = false;
        _videoPlayer.isLooping = loop;
        _videoPlayer.skipOnDrop = skipOnDrop;
        _videoPlayer.aspectRatio = aspectRatio;
        _videoPlayer.SetDirectAudioVolume(0, defaultVolume);

        // 设置渲染模式为RenderTexture以获得最佳兼容性
        if (_videoPlayer.renderMode == VideoRenderMode.CameraFarPlane ||
            _videoPlayer.renderMode == VideoRenderMode.CameraNearPlane)
        {
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        }
    }

    /// <summary>
    /// 订阅视频播放器事件
    /// </summary>
    private void SubscribeEvents()
    {
        _videoPlayer.started += OnVideoStarted;
        _videoPlayer.loopPointReached += OnVideoCompleted;
        _videoPlayer.errorReceived += OnVideoError;
        _videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeEvents()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.started -= OnVideoStarted;
            _videoPlayer.loopPointReached -= OnVideoCompleted;
            _videoPlayer.errorReceived -= OnVideoError;
            _videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }

    private void OnVideoStarted(VideoPlayer vp) => OnPlayStarted?.Invoke();

    private void OnVideoCompleted(VideoPlayer vp) => OnPlayCompleted?.Invoke();

    private void OnVideoError(VideoPlayer vp, string message) => OnError?.Invoke(message);

    private void OnVideoPrepared(VideoPlayer vp) => Debug.Log("视频准备完成");

    // 播放控制

    public void Play()
    {
        if (!_isInitialized)
        {
            OnError?.Invoke("播放器未初始化");
            return;
        }

        try
        {
            _videoPlayer.Play();
            StartProgressCoroutine();
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"播放失败: {ex.Message}");
        }
    }

    public void Pause()
    {
        if (_videoPlayer != null && _videoPlayer.isPlaying)
        {
            _videoPlayer.Pause();
            StopProgressCoroutine();
            OnPlayPaused?.Invoke();
        }
    }

    public void Stop()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.Stop();
            StopProgressCoroutine();
            OnPlayStopped?.Invoke();
        }
    }

    // 进度控制
    public void SetProgress(float progress)
    {
        if (_videoPlayer == null) return;

        progress = Mathf.Clamp01(progress);
        _targetProgress = progress;

        if (_videoPlayer.isPrepared)
        {
            _videoPlayer.time = _videoPlayer.length * progress;
        }
    }

    public float GetProgress()
    {
        if (_videoPlayer == null || _videoPlayer.length <= 0) return 0f;
        return (float)(_videoPlayer.time / _videoPlayer.length);
    }

    // 音量控制

    public void SetVolume(float volume)
    {
        if (_videoPlayer != null)
        {
            volume = Mathf.Clamp01(volume);
            _videoPlayer.SetDirectAudioVolume(0, volume);
        }
    }

    public float GetVolume()
    {
        return _videoPlayer != null ? _videoPlayer.GetDirectAudioVolume(0) : 0f;
    }

    // 视频源设置

    /// <summary>
    /// 设置本地视频文件
    /// </summary>
    /// <param name="videoClip"></param>
    public void SetVideoClip(VideoClip videoClip)
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.source = VideoSource.VideoClip;
            _videoPlayer.clip = videoClip;
            PrepareVideo();
        }
    }

    /// <summary>
    /// 设置网络视频URL
    /// </summary>
    /// <param name="url"></param>
    public void SetVideoURL(string url)
    {
        if (_videoPlayer != null && !string.IsNullOrEmpty(url))
        {
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = url;
            PrepareVideo();
        }
    }

    private void PrepareVideo()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.Prepare();
        }
    }

    // 进度更新协程

    private void StartProgressCoroutine()
    {
        StopProgressCoroutine();
        _progressUpdateCoroutine = StartCoroutine(UpdateProgressCoroutine());
    }

    private void StopProgressCoroutine()
    {
        if (_progressUpdateCoroutine != null)
        {
            StopCoroutine(_progressUpdateCoroutine);
            _progressUpdateCoroutine = null;
        }
    }

    private IEnumerator UpdateProgressCoroutine()
    {
        while (_videoPlayer != null && _videoPlayer.isPlaying)
        {
            // 处理设定的目标进度
            if (_targetProgress >= 0f && _videoPlayer.isPrepared)
            {
                _videoPlayer.time = _videoPlayer.length * _targetProgress;
                _targetProgress = -1f;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public void AdaptRawImageAspectRatio(RawImage rawImage, float videoWidth, float videoHeight)
    {
        if (rawImage == null || rawImage.rectTransform == null)
            return;

        float videoAspect = videoWidth / videoHeight;
        float parentWidth = rawImage.rectTransform.parent.GetComponent<RectTransform>().rect.width;
        float parentHeight = rawImage.rectTransform.parent.GetComponent<RectTransform>().rect.height;
        float parentAspect = parentWidth / parentHeight;

        if (videoAspect > parentAspect)
        {
            // 以宽度为基准，调整高度
            float height = parentWidth / videoAspect;
            rawImage.rectTransform.sizeDelta = new Vector2(parentWidth, height);
        }
        else
        {
            // 以高度为基准，调整宽度
            float width = parentHeight * videoAspect;
            rawImage.rectTransform.sizeDelta = new Vector2(width, parentHeight);
        }
    }

}
