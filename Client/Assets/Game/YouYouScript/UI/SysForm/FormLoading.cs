using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


/// <summary>
/// "加载"界面
/// </summary>
public class FormLoading : UIFormBase
{
    [SerializeField]
    private Scrollbar m_Scrollbar;
    [SerializeField]
    private Text txtTip;

    private void OnLoadingProgressChange(object userData)
    {
        VarFloat varFloat = (VarFloat)userData;
        txtTip.text = string.Format("正在进入场景, 加载进度 {0}%", Math.Floor(varFloat * 100));
        GameUtil.LogError("进度条:",varFloat.Value);
        m_Scrollbar.size = varFloat.Value;
        if (varFloat == 1) Close();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEntry.Event.AddEventListener(Constants.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
        m_Scrollbar.size = 0;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        GameEntry.Event.RemoveEventListener(Constants.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
        m_Scrollbar.size = 0f;
    }
}
