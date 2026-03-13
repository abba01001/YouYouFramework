using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// "加载"界面
/// </summary>
public class FormLoading : UIFormBase
{
    [SerializeField] private RectTransform m_value;
    [SerializeField] private RectTransform m_valueBg;
    [SerializeField] private Text txtTip;

    private float m_TargetProgress;

    private void OnLoadingProgressChange(object userData)
    {
        VarFloat m_TargetProgress = userData as VarFloat;
        txtTip.text = string.Format("正在进入场景, 加载进度 {0}%", Math.Floor(m_TargetProgress.Value * 100));
        m_value.sizeDelta = new Vector2((float) m_TargetProgress.Value * m_valueBg.sizeDelta.x, 42);
        if (m_TargetProgress == 1) Close();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEntry.Event.AddEventListener(Constants.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
        m_value.sizeDelta = new Vector2(0, 42);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEntry.Event.RemoveEventListener(Constants.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
        m_value.sizeDelta = new Vector2(0, 42);
    }
}