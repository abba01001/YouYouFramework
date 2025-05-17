using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Main;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

/// <summary>
/// 提示窗口
/// </summary>
public class FormChangeName : UIFormBase
{
    [SerializeField] private InputField _inputField;
    [SerializeField] private Button btn;
    protected override void OnEnable()
    {
        base.OnEnable();
        btn.SetButtonClick(() =>
        {
            if (_inputField.text == "")
            {
                return;
            }
            GameEntry.Data.PlayerRoleData.name = _inputField.text;
            GameEntry.UI.CloseUIForm<FormChangeName>();
            GameEntry.Event.Dispatch(Constants.EventName.TriggerGuideEvent,Constants.EventName.FinishInputName);
        });
    }

    protected override void OnShow()
    {
        base.OnShow();

    }
    
}
