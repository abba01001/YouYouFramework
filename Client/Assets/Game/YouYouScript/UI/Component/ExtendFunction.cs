using System;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public static class ExtendFunction
    {
        public static T Get<T>(this GameObject go, string path) where T : Component => go.transform.Get<T>(path);
        public static T Get<T>(this Transform ts, string path) where T : Component
        {
            Transform result = ts.Find(path);
            if (result == null)
            {
                GameEntry.LogError($"该路径未找到对应的组件{path}");
                return null;
            }
            return result.GetComponent<T>();
        }

        public static Transform FindOrNull(this Transform ts, string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return ts.Find(path);
        }

        public static void SetButtonClick(this Button btn, UnityEngine.Events.UnityAction action, bool isClear = true)
        {
            if (isClear) btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                var clickTime = btn.GetComponent<BtnClickTimeMono>() ?? btn.gameObject.AddComponent<BtnClickTimeMono>();
                if (clickTime.CanClick(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()))
                    action?.Invoke();
            });
        }

        public static void MSetActive(this GameObject go, bool State)
        {
            if (go)
            {
                if (go.activeSelf != State)
                {
                    go.SetActive(State);
                }
            }
        }
        
        public static void ShowRemind(this GameObject button,bool show)
        {
            Transform redDot = button.transform.Find("Remind");
            if(redDot != null)
            {
                redDot.gameObject.MSetActive(show);
            }
        }
    }