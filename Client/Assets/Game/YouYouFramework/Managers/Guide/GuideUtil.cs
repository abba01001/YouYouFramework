using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GuideUtil
{
    public static void CheckDirectNext(Action onNext = null)
    {
        GameEntry.Log(LogCategory.Guide, "CheckDirectNext");
        onNext?.Invoke();
        GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
    }

    /// <summary>
    /// 监听按钮点击, 触发下一步
    /// </summary>
    public static void CheckBtnNext(GameObject button, Action onNext = null)
    {
        if (button == null) return;
        button.GetComponent<Button>().onClick.AddListener(OnNext);
        GameEntry.Log(LogCategory.Guide, "CheckBtnNext");

        void OnNext()
        {
            GameEntry.Log(LogCategory.Guide, "CheckBtnNext-OnNext");
            button.GetComponent<Button>().onClick.RemoveListener(OnNext);

            onNext?.Invoke();
            GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
        }
    }

    /// <summary>
    /// 监听开关激活, 触发下一步
    /// </summary>
    public static void CheckToggleNext(Toggle toggle)
    {
        toggle.onValueChanged.AddListener(OnNext);

        void OnNext(bool isOn)
        {
            if (!isOn) return;
            toggle.onValueChanged.RemoveListener(OnNext);
            GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
        }
    }

    /// <summary>
    /// 监听事件, 触发下一步
    /// </summary>
    public static void CheckEventNext(string eventName)
    {
        GameEntry.Event.Common.AddEventListener(eventName, OnNext);

        void OnNext(object userData)
        {
            GameEntry.Event.Common.RemoveEventListener(eventName, OnNext);
            GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
        }
    }
}