using System;
using UnityEngine;
using UnityEngine.U2D;
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
        
        public static void SetSpriteByAtlas(this Component component, string key, string name, bool needSetNative = true, Action action = null)
        {
            
            SetSpriteAsyncDetail(GameEntry.Atlas.GetAtlas(key), component, name, needSetNative, action);
        }
        
        private static void SetSpriteAsyncDetail(SpriteAtlas atlas, Component component, string name, bool needSetNative = true, Action action = null)
        {
            switch (component)
            {
                case Image image:
                    image.sprite = atlas.GetSprite(name);
                    image.enabled = true;
                    if (needSetNative) image.SetNativeSize();
                    break;
                case SpriteRenderer spriteRenderer:
                    spriteRenderer.sprite = atlas.GetSprite(name);
                    spriteRenderer.enabled = true;
                    break;
                case RawImage rawImage:
                    rawImage.texture = atlas.GetSprite(name).texture;
                    rawImage.enabled = true;
                    break;
                default:
                    break;
            }
            action?.Invoke();
        }
    }