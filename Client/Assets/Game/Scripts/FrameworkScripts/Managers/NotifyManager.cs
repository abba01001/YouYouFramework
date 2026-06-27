using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

namespace GameScripts
{
    public class NotifyManager
    {
        private const string CHANNEL_ID = "default_channel";
        private const string PREFS_KEY = "Notification_Map";
        private Dictionary<string, int> _activeNotificationMap = new Dictionary<string, int>();

        public NotifyManager()
        {
            LoadNotificationMap();
            RegisterNotificationChannel();
        }

        private void RegisterNotificationChannel()
        {
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel(CHANNEL_ID, "Game Notifications", "游戏通知", Importance.Default);
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
        }

        public void Schedule(NotificationRequest request)
        {
#if UNITY_ANDROID
            string today = System.DateTime.Now.ToString("yyyy-MM-dd");
            string prefKey = $"LastScheduled_{request.Id}";
            if (PlayerPrefs.GetString(prefKey, "") == today) return;

            if (_activeNotificationMap.TryGetValue(request.Id, out int oldId))
            {
                AndroidNotificationCenter.CancelNotification(oldId);
            }

            var notification = new AndroidNotification
            {
                Title = request.Title,
                Text = request.Content,
                FireTime = System.DateTime.Now.AddSeconds(request.DelaySeconds)
            };

            int newId = AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
            _activeNotificationMap[request.Id] = newId;
            SaveNotificationMap();
            PlayerPrefs.SetString(prefKey, today);
            PlayerPrefs.Save();
#endif
        }

        private void SaveNotificationMap()
        {
            List<string> data = new List<string>();
            foreach (var kvp in _activeNotificationMap) data.Add($"{kvp.Key}:{kvp.Value}");
            PlayerPrefs.SetString(PREFS_KEY, string.Join("|", data.ToArray()));
        }

        public void ClearNotificationMap()
        {
            PlayerPrefs.SetString(PREFS_KEY, "");
        }
        
        private void LoadNotificationMap()
        {
            string data = PlayerPrefs.GetString(PREFS_KEY, "");
            if (string.IsNullOrEmpty(data)) return;
            foreach (var entry in data.Split('|'))
            {
                string[] parts = entry.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int id)) _activeNotificationMap[parts[0]] = id;
            }
        }
    }
}


[System.Serializable]
public class NotificationRequest
{
    public string Id;          // 唯一ID
    public string Title;       // 通知标题
    public string Content;     // 通知正文
    public int DelaySeconds;   // 延迟触发秒数
}