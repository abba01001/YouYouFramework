using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Main;
using UnityEngine;
using UnityEngine.Networking;

namespace YouYou
{
    public class TimeManager
    {
        /// <summary>
        /// 定时器排序字典
        /// </summary>
        private readonly SortedDictionary<float, List<TimeAction>> m_SortedDictionary;
        private readonly SortedDictionary<float, List<TimeAction>> m_UnscaledSortedDictionary;

        private readonly Queue<float> timeOutTime = new Queue<float>();

        // 记录最近的TimeAction的临时目标时间点
        private float minTime;
        private float unscaledMinTime;
        private long? netTime;
        private TimeAction netTimeAction;
        public void SetNetTime(long time)
        {
            netTime = time;
            RefreshNetTime();
        }

        public void Init()
        {
            string url = "https://www.baidu.com";
            GetServerTime(url, 30, null, onComplete: (DateTime serverTime) =>
            {
                long time = GameEntry.Time.DateTimeToUnixTimestamp(serverTime);
                SetNetTime(time);
            });
        }

        public async void GetServerTime(string url, int tryRequestCount, Action onUpdate, Action<DateTime> onComplete)
        {
            int currentRetry = 0;

            while (currentRetry < tryRequestCount)
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    try
                    {
                        onUpdate?.Invoke();
                        await webRequest.SendWebRequest();
                        if (webRequest.responseCode == 404)
                        {
                            onComplete?.Invoke(DateTime.Now.ToLocalTime());
                            return; // 遇到 404 错误直接退出
                        }

                        if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                            webRequest.result == UnityWebRequest.Result.ProtocolError)
                        {
                            currentRetry++;
                            await UniTask.Delay(100); // 等待 0.1 秒
                        }
                        else
                        {
                            // 处理成功响应
                            string dateStr = webRequest.GetResponseHeader("Date");
                            if (DateTime.TryParse(dateStr, out DateTime serverTime))
                            {
                                onComplete?.Invoke(serverTime);
                                MainEntry.Log(MainEntry.LogCategory.NetWork, "获取服务器时间成功 {0}", serverTime.ToLocalTime());
                            }
                            else
                            {
                                onComplete?.Invoke(DateTime.Now.ToLocalTime());
                            }

                            return; // 成功后退出方法
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("404"))
                        {
                            onComplete?.Invoke(DateTime.Now.ToLocalTime());
                            return;
                        }
                    }
                }
            }
            onComplete?.Invoke(DateTime.Now.ToLocalTime());
        }
        

        public long GetNetTime()
        {
            if (netTime.HasValue) return netTime.Value;
            return DateTimeToUnixTimestamp(DateTime.Now);
        }

        public DateTime GetNetDate()
        {
            if (netTime.HasValue) return UnixTimestampToDateTime(netTime.Value);
            return DateTime.Now;
        }
        
        public long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime.ToLocalTime());
            return dateTimeOffset.ToUnixTimeSeconds();
        }
        
        public DateTime UnixTimestampToDateTime(long unixTimestamp)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            return dateTimeOffset.UtcDateTime.ToLocalTime();
        }
        
        internal TimeManager()
        {
            m_SortedDictionary = new SortedDictionary<float, List<TimeAction>>();
            m_UnscaledSortedDictionary = new SortedDictionary<float, List<TimeAction>>();
        }

        private void RefreshNetTime()
        {
            netTimeAction?.Stop();
            netTimeAction = CreateTimerLoop(this, 1f, -1, (int loop) =>
            {
                if (netTime.HasValue)
                {
                    long newTime = netTime.Value + 1;
                    netTime = newTime;
                }
            });
        }

        /// <summary>
        /// 注册定时器
        /// </summary>
        internal void Register(float tillTime, TimeAction action, bool unScaled)
        {
            if (unScaled)
            {
                if (!m_UnscaledSortedDictionary.ContainsKey(tillTime)) m_UnscaledSortedDictionary.Add(tillTime, new List<TimeAction>());
                m_UnscaledSortedDictionary[tillTime].Add(action);
                if (tillTime < unscaledMinTime) unscaledMinTime = tillTime;
            }
            else
            {
                if (m_SortedDictionary.TryGetValue(tillTime, out List<TimeAction> lst))
                {
                    lst.Add(action);
                }
                else
                {
                    lst = new List<TimeAction>();
                    lst.Add(action);
                    m_SortedDictionary.Add(tillTime, lst);

                }
                if (tillTime < minTime) minTime = tillTime;
            }
        }
        /// <summary>
        /// 移除定时器
        /// </summary>
        internal void Remove(float tillTime, TimeAction action, bool unScaled)
        {
            if (unScaled)
            {
                if (m_UnscaledSortedDictionary.TryGetValue(tillTime, out List<TimeAction> lst))
                {
                    lst.Remove(action);
                }
            }
            else
            {
                if (m_SortedDictionary.TryGetValue(tillTime, out List<TimeAction> lst))
                {
                    lst.Remove(action);
                }
            }
        }

        internal void OnUpdate()
        {
            if (m_SortedDictionary.Count > 0 && Time.time >= minTime)
            {
                var enumerator = m_SortedDictionary.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    float k = enumerator.Current.Key;
                    if (k > Time.time)
                    {
                        minTime = k;
                        break;
                    }
                    timeOutTime.Enqueue(k);
                }
                foreach (var item in timeOutTime)
                {
                    if (m_SortedDictionary.TryGetValue(item, out List<TimeAction> lst))
                    {
                        for (int i = 0; i < lst.Count; i++)
                        {
                            lst[i].TillTimeEnd();
                        }
                        m_SortedDictionary.Remove(item);
                    }
                }
                timeOutTime.Clear();
            }

            if (m_UnscaledSortedDictionary.Count > 0 && Time.unscaledTime >= unscaledMinTime)
            {
                foreach (var item in m_UnscaledSortedDictionary)
                {
                    float k = item.Key;
                    if (k > Time.unscaledTime)
                    {
                        unscaledMinTime = item.Key;
                        break;
                    }
                    timeOutTime.Enqueue(k);
                }
                foreach (var item in timeOutTime)
                {
                    if (m_UnscaledSortedDictionary.ContainsKey(item) == false) continue;

                    List<TimeAction> lst = m_UnscaledSortedDictionary[item];
                    for (int i = 0; i < lst.Count; i++)
                    {
                        lst[i].TillTimeEnd();
                    }
                    m_UnscaledSortedDictionary.Remove(item);
                }
                timeOutTime.Clear();
            }
            
        }

        /// <summary>
        /// 创建定时器，可循环多次
        /// </summary>
        /// <param name="interval">间隔时间</param>
        /// <param name="loop">循环次数</param>
        /// <param name="onUpdate">循环一次时调用</param>
        /// <param name="onComplete">全部循环完毕时调用</param>
        /// <param name="unScaled">是否无视Time.timeScale</param>
        /// <returns>定时器</returns>
        public TimeAction CreateTimerLoop(object target, float interval, int loop = 1, Action<int> onUpdate = null, Action onComplete = null, bool unScaled = false)
        {
            return new TimeAction().Init(target, interval, loop, onUpdate, onComplete, unScaled);
        }

        /// <summary>
        /// 等待n秒 只执行一次
        /// </summary>
        public TimeAction CreateTimer(object target, float delayTime, Action onComplete, bool unScaled = false)
        {
            return new TimeAction().Init(target, delayTime, 1, null, onComplete, unScaled);
        }

        /// <summary>
        /// 等待n秒 只执行一次
        /// </summary>
        public UniTask Delay(object target, float delayTime, bool unScaled = false)
        {
            var task = new UniTaskCompletionSource();
            CreateTimer(target, delayTime, () => task.TrySetResult(), unScaled);
            return task.Task;
        }

        /// <summary>
        /// 延迟一帧
        /// </summary>
        public void Yield(Action onComplete)
        {
            GameEntry.Instance.StartCoroutine(YieldCoroutine());
            IEnumerator YieldCoroutine()
            {
                yield return null;
                onComplete?.Invoke();
            }
        }

        public void SetTimeScale(float scale)
        {
            Time.timeScale = scale;
        }
    }
}