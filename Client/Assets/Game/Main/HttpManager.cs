using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Main
{
    // 错误类型枚举，方便精准定位问题
    public enum HttpErrorType
    {
        None,
        NetworkError,   // 网络连接、DNS失败
        Timeout,        // 超时
        HttpError,      // HTTP状态码错误 (404, 500等)
        Cancelled,      // 请求被取消
        Unknown         // 未知错误
    }

    public class HttpCallBackArgs : EventArgs
    {
        public HttpErrorType ErrorType;
        public string ErrorMessage;
        public long StatusCode; // 记录状态码 (200, 404, 500等)
        public string Value;
        public byte[] Data;

        public bool HasError => ErrorType != HttpErrorType.None;
    }

    public class HttpManager : MonoBehaviour
    {
        public static HttpManager Instance { get; private set; }

        [Header("Settings")]
        public int HttpRetry = 3;
        public float HttpRetryInterval = 1f;
        public int Timeout = 5;

        private readonly Stack<HttpRoutine> m_Pool = new Stack<HttpRoutine>();
        private readonly HashSet<string> m_RunningRequests = new HashSet<string>();

        private void Awake() => Instance = this;

        public HttpRoutine DequeueRoutine()
        {
            return m_Pool.Count > 0 ? m_Pool.Pop() : new HttpRoutine(HttpRetry, HttpRetryInterval, Timeout);
        }

        public void EnqueueRoutine(HttpRoutine routine)
        {
            routine.Reset();
            m_Pool.Push(routine);
        }

        public async UniTask<HttpCallBackArgs> GetAsync(string url)
        {
            Debugger.Log("HttpManager GetAsync==>",url);
            if (string.IsNullOrEmpty(url)) return new HttpCallBackArgs { ErrorType = HttpErrorType.Unknown, ErrorMessage = "Empty URL" };

            if (!m_RunningRequests.Add(url))
            {
                Debug.LogWarning($"[HttpManager] 重复请求已拦截: {url}");
                return new HttpCallBackArgs { ErrorType = HttpErrorType.Unknown, ErrorMessage = "Request in progress" };
            }

            try
            {
                var routine = DequeueRoutine();
                return await routine.GetAsync(url, this);
            }
            finally
            {
                m_RunningRequests.Remove(url);
            }
        }

        public async UniTask<HttpCallBackArgs> PostAsync(string url, string json)
        {
            var routine = DequeueRoutine();
            return await routine.PostAsync(url, json, this);
        }

        public async UniTask<string> GetStringAsync(string url)
        {
            var result = await GetAsync(url);
            return !result.HasError ? result.Value : null;
        }
    }

    public class HttpRoutine
    {
        private int m_MaxRetry;
        private float m_RetryInterval;
        private int m_Timeout;
        private int m_CurrRetry = 0;
        private string m_Url;
        private string m_Json;

        public HttpRoutine(int maxRetry, float retryInterval, int timeout)
        {
            m_MaxRetry = maxRetry;
            m_RetryInterval = retryInterval;
            m_Timeout = timeout;
        }

        public void Reset() => m_CurrRetry = 0;

        public async UniTask<HttpCallBackArgs> GetAsync(string url, HttpManager manager)
        {
            m_Url = url;
            return await ExecuteRequest(UnityWebRequest.Get(url), manager);
        }

        public async UniTask<HttpCallBackArgs> PostAsync(string url, string json, HttpManager manager)
        {
            m_Url = url;
            m_Json = json;
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json ?? ""));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            return await ExecuteRequest(request, manager);
        }

        private async UniTask<HttpCallBackArgs> ExecuteRequest(UnityWebRequest request, HttpManager manager)
        {
            request.timeout = m_Timeout;
            var result = await request.SendWebRequest().ToUniTask();

            if (result.result == UnityWebRequest.Result.Success)
            {
                var args = new HttpCallBackArgs { ErrorType = HttpErrorType.None, Value = result.downloadHandler.text, Data = result.downloadHandler.data };
                Finish(request, manager);
                return args;
            }

            if (m_CurrRetry < m_MaxRetry)
            {
                m_CurrRetry++;
                await UniTask.Delay(System.TimeSpan.FromSeconds(m_RetryInterval));
                return await (request.method == UnityWebRequest.kHttpVerbGET ? GetAsync(m_Url, manager) : PostAsync(m_Url, m_Json, manager));
            }

            // 错误分类映射
            var errType = HttpErrorType.Unknown;
            if (result.result == UnityWebRequest.Result.ConnectionError) errType = HttpErrorType.NetworkError;
            else if (result.result == UnityWebRequest.Result.ProtocolError) errType = HttpErrorType.HttpError;
            else if (result.result == UnityWebRequest.Result.DataProcessingError) errType = HttpErrorType.Timeout;

            var errArgs = new HttpCallBackArgs 
            { 
                ErrorType = errType, 
                ErrorMessage = result.error, 
                StatusCode = request.responseCode 
            };
            Finish(request, manager);
            return errArgs;
        }

        private void Finish(UnityWebRequest request, HttpManager manager)
        {
            request.Dispose();
            manager.EnqueueRoutine(this);
        }
    }
}