using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using YouYou;
using Object = UnityEngine.Object;

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

    public static void ShowRemind(this GameObject button, bool show)
    {
        Transform redDot = button.transform.Find("Remind");
        if (redDot != null)
        {
            redDot.gameObject.MSetActive(show);
        }
    }

    public static void SetSpriteByAtlas(this Component component, string key, string name, bool needSetNative = true,
        Action action = null)
    {
        SetSpriteAsyncDetail(GameEntry.Atlas.GetAtlas(key), component, name, needSetNative, action);
    }

    private static void SetSpriteAsyncDetail(SpriteAtlas atlas, Component component, string name,
        bool needSetNative = true, Action action = null)
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
    
    /// <summary>
    /// 获取或创建组件
    /// </summary>
    public static T GetOrCreatComponent<T>(this GameObject obj) where T : MonoBehaviour
    {
        T t = obj.GetComponent<T>();
        if (t == null)
        {
            t = obj.AddComponent<T>();
        }
        return t;
    }
    /// <summary>
    /// 设置当前gameObject及所有子物体的层
    /// </summary>
    public static void SetLayer(this GameObject obj, string layerName)
    {
        Transform[] transArr = obj.transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < transArr.Length; i++)
        {
            transArr[i].gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    /// <summary>
    /// 自动加载图片
    /// </summary>
    public static async void AutoLoadTexture(this Image img, string imgPath, bool isSetNativeSize = false)
    {
        Object asset = await GameEntry.Loader.LoadMainAssetAsync<Object>(imgPath, img.gameObject);
        Sprite obj = null;
        if (asset is Sprite)
        {
            obj = (Sprite)asset;
        }
        else
        {
            Texture2D texture = (Texture2D)asset;
            obj = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        img.sprite = obj;
        if (isSetNativeSize) img.SetNativeSize();
    }
    /// <summary>
    /// 自动加载图片
    /// </summary>
    public static async void AutoLoadTexture(this RawImage img, string imgPath, bool isSetNativeSize = false)
    {
        Texture2D asset = await GameEntry.Loader.LoadMainAssetAsync<Texture2D>(imgPath, img.gameObject);
        img.texture = asset;
        if (isSetNativeSize) img.SetNativeSize();
    }

    /// <summary>
    /// 设置特效渲染层级
    /// </summary>
    public static void SetEffectOrder(this Transform trans, int sortingOrder)
    {
        Renderer[] renderers = trans.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++) renderers[i].sortingOrder = sortingOrder;
    }
    
    public static int ToInt(this object value, int defaultValue = 0)
    {
        int temp = defaultValue;
        if (value != null) int.TryParse(value.ToString(), out temp);
        return temp;
    }
    public static float ToFloat(this object value, float defaultValue = 0)
    {
        float temp = defaultValue;
        if (value != null) float.TryParse(value.ToString(), out temp);
        return temp;
    }

    public static bool CustomContains<T>(this IList<T> list, T t) where T : class
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == t) return true;
        }
        return false;
    }
}