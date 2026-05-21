using Cysharp.Threading.Tasks;
using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GameScripts
{
    public class HttpManager: MonoBehaviour, ISingleton
    {
        public static HttpManager Instance => MonoSingletonProperty<HttpManager>.Instance;
        public void OnSingletonInit()
        {
            Init();
        }
        public void OnSingletonDispose() { }
        
        /// <summary>
        /// 正式账号服务器Url
        /// </summary>
        private string m_WebAccountUrl;
    
        /// <summary>
        /// 测试账号服务器Url
        /// </summary>
        private string m_TestWebAccountUrl;
    
        /// <summary>
        /// 是否测试环境
        /// </summary>
        private bool m_IsTest;
    
        /// <summary>
        /// 真实账号服务器Url
        /// </summary>
        public string RealWebAccountUrl
        {
            get { return "http://" + RealIpAndPort + "/"; }
        }
    
        public string RealIpAndPort
        {
            get { return m_IsTest ? m_TestWebAccountUrl : m_WebAccountUrl; }
        }
    
    
        internal void Init()
        {
            m_WebAccountUrl = GameEntry.ParamsSettings.ServerWebUrl;
            m_TestWebAccountUrl = GameEntry.ParamsSettings.LocalWebUrl;
        }
    
        #region Get
    
        public void GetArgs(string url, bool loadingCircle = false, HttpSendDataCallBack callBack = null)
        {
            GameEntry.Task.AddTaskCommon((taskRoutine) =>
            {
                HttpRoutine.Create().Get(url, (HttpCallBackArgs ret) =>
                {
                    taskRoutine.Leave();
                    if (ret.HasError)
                    {
                        Debug.LogError("UITipLogOut");
                        //GameEntry.Instance.UITipLogOut.SetValue(() => GetArgs(url, loadingCircle, callBack));
                    }
                    else
                    {
                        callBack?.Invoke(ret);
                    }
                });
            }, loadingCircle);
        }
    
        public void Get(string url, bool loadingCircle = false, Action<string> callBack = null)
        {
            GetArgs(url, loadingCircle, (args) =>
            {
                if (args.Value.JsonCutApart("status").ToInt() == 1) callBack?.Invoke(args.Value.JsonCutApart("data"));
            });
        }
    
        public UniTask<HttpCallBackArgs> GetArgsAsync(string url, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<HttpCallBackArgs>();
            GetArgs(url, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }
    
        public UniTask<string> GetAsync(string url, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<string>();
            Get(url, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }
    
        #endregion
    
        #region Post
    
        public void PostArgs(string url, string json = null, bool loadingCircle = false,
            HttpSendDataCallBack callBack = null)
        {
            GameEntry.Task.AddTaskCommon((taskRoutine) =>
            {
                HttpRoutine.Create().Post(url, json, (HttpCallBackArgs ret) =>
                {
                    taskRoutine.Leave();
                    if (ret.HasError)
                    {
                        Debug.LogError("UITipLogOut");
                        //GameEntry.Instance.UITipLogOut.SetValue(() => PostArgs(url, json, loadingCircle, callBack));
                    }
                    else
                    {
                        Debugger.LogError(ret);
                        callBack?.Invoke(ret);
                    }
                });
            }, loadingCircle);
        }
    
        public void Post(string url, string json = null, bool loadingCircle = false, Action<string> callBack = null)
        {
            PostArgs(url, json, loadingCircle, (args) =>
            {
                callBack?.Invoke(args.Value.JsonCutApart("content"));
                if (args.Value.JsonCutApart("status").ToInt() == 1) callBack?.Invoke(args.Value.JsonCutApart("data"));
            });
        }
    
        public UniTask<HttpCallBackArgs> PostArgsAsync(string url, string json = null, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<HttpCallBackArgs>();
            PostArgs(url, json, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }
    
        public UniTask<string> PostAsync(string url, string json = null, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<string>();
            Post(url, json, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }
    
        #endregion
    }
    
    
    
        /// <summary>
    /// Http发送数据的回调委托
    /// </summary>
    /// <param name="args"></param>
    public delegate void HttpSendDataCallBack(HttpCallBackArgs args);
    
    /// <summary>
    /// Http访问器
    /// </summary>
    public class HttpRoutine
    {
        #region 属性
    
        /// <summary>
        /// Http请求回调
        /// </summary>
        private HttpSendDataCallBack m_CallBack;
    
        /// <summary>
        /// Http请求回调数据
        /// </summary>
        private HttpCallBackArgs m_CallBackArgs;
    
        /// <summary>
        /// 是否繁忙
        /// </summary>
        public bool IsBusy { get; private set; }
    
        /// <summary>
        /// 当前重试次数
        /// </summary>
        private int m_CurrRetry = 0;
    
        private string m_Url;
        private string m_Json;
    
        /// <summary>
        /// 发送的数据
        /// </summary>
        private Dictionary<string, object> m_Dic;
        #endregion
    
        public HttpRoutine()
        {
            m_CallBackArgs = new HttpCallBackArgs();
            m_Dic = new Dictionary<string, object>();
        }
        public static HttpRoutine Create()
        {
            return GameEntry.ClassObjectPool.Dequeue<HttpRoutine>();
        }
    
        #region SendData 发送web数据
        /// <summary>
        /// 发送web数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callBack"></param>
        /// <param name="isPost"></param>
        /// <param name="isGetData">是否获取字节数据</param>
        /// <param name="dic"></param>
        public void Get(string url, HttpSendDataCallBack callBack = null)
        {
            if (IsBusy) return;
            IsBusy = true;
    
            m_Url = url;
            m_CallBack = callBack;
    
            GetUrl(m_Url);
        }
    
        public void Post(string url, string json = null, HttpSendDataCallBack callBack = null)
        {
            if (IsBusy) return;
            IsBusy = true;
    
            m_Url = url;
            m_CallBack = callBack;
            m_Json = json;
    
            PostUrl(m_Url);
        }
        #endregion
    
        #region GetUrl Get请求
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        private void GetUrl(string url)
        {
            Debugger.Log( "Get请求:{0}, {1}次重试", m_Url, m_CurrRetry);
            UnityWebRequest data = UnityWebRequest.Get(url);
            MainEntry.Instance.StartCoroutine(Request(data));
        }
        #endregion
    
        #region PostUrl Post请求
        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        private void PostUrl(string url)
        {
            UnityWebRequest unityWeb = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            unityWeb.downloadHandler = new DownloadHandlerBuffer();
            if (!string.IsNullOrWhiteSpace(m_Json))
            {
                if (GameEntry.ParamsSettings.PostIsEncrypt && m_CurrRetry == 0)
                {
                    m_Dic["value"] = m_Json;
                    //web加密
                    m_Dic["deviceIdentifier"] = AndroidHelper.DeviceIdentifier;
                    m_Dic["deviceModel"] = AndroidHelper.DeviceModel;
                    long t = DateTime.Now.Ticks;
                    m_Dic["sign"] = EncryptUtil.Md5(string.Format("{0}:{1}", t, AndroidHelper.DeviceIdentifier));
                    m_Dic["t"] = t;
    
                    m_Json = m_Dic.ToJson();
                }
                unityWeb.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(m_Json));
    
                if (!string.IsNullOrWhiteSpace(GameEntry.ParamsSettings.PostContentType))
                    unityWeb.SetRequestHeader("Content-Type", GameEntry.ParamsSettings.PostContentType);
            }
    
            Debugger.Log("Post请求:{0}, {1}次重试==>>{2}", m_Url, m_CurrRetry, m_Json);
            MainEntry.Instance.StartCoroutine(Request(unityWeb));
        }
        #endregion
    
        #region Request 请求服务器
        /// <summary>
        /// 请求服务器
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private IEnumerator Request(UnityWebRequest data)
        {
            data.timeout = 5;
            yield return data.SendWebRequest();
            if (data.result == UnityWebRequest.Result.Success)
            {
                IsBusy = false;
                m_CallBackArgs.HasError = false;
                m_CallBackArgs.Value = data.downloadHandler.text;
                m_CallBackArgs.Data = data.downloadHandler.data;
            }
            else
            {
                //报错了 进行重试
                if (m_CurrRetry > 0) yield return new WaitForSeconds(GameEntry.HttpRetryInterval);
                m_CurrRetry++;
                if (m_CurrRetry <= GameEntry.HttpRetry)
                {
                    switch (data.method)
                    {
                        case UnityWebRequest.kHttpVerbGET:
                            GetUrl(m_Url);
                            break;
                        case UnityWebRequest.kHttpVerbPOST:
                            PostUrl(m_Url);
                            break;
                    }
                    yield break;
                }
    
                IsBusy = false;
                m_CallBackArgs.HasError = true;
                m_CallBackArgs.Value = data.error;
            }
    
            //if (!string.IsNullOrWhiteSpace(m_CallBackArgs.Value)) Debugger.Log( "WebAPI回调:{0}, ==>>{1}", m_Url, m_CallBackArgs.ToJson());
            m_CallBack?.Invoke(m_CallBackArgs);
    
            m_CurrRetry = 0;
            m_Url = null;
            if (m_Dic != null)
            {
                m_Dic.Clear();
                GameEntry.ClassObjectPool.Enqueue(m_Dic);
            }
            m_CallBackArgs.Data = null;
            data.Dispose();
            data = null;
    
            //Debug.Log("把http访问器回池");
            GameEntry.ClassObjectPool.Enqueue(this);
        }
        #endregion
    }
    
    
    /// <summary>
    /// Http请求回调数据类 用于存储从Http站点下载的数据
    /// </summary>
    public class HttpCallBackArgs : EventArgs
    {
        /// <summary>
        /// 是否有错
        /// </summary>
        public bool HasError;
    
        /// <summary>
        /// Json返回值
        /// </summary>
        public string Value;
    
        /// <summary>
        /// bytes返回值
        /// </summary>
        public byte[] Data;
    }
}