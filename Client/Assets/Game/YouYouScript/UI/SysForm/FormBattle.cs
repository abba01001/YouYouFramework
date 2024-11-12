using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


/// <summary>
/// "战斗"界面
/// </summary>
public class FormBattle : UIFormBase
{
    [SerializeField] private YouYouJoystick MoveJoystick;
    [SerializeField] private YouYouJoystick RotateJoystick;
    [SerializeField] private Image jumpBtn;
    public YouYouJoystick GetMoveJoystick()
    {
        return MoveJoystick;
    }
    
    public YouYouJoystick GetRotateJoystick()
    {
        return RotateJoystick;
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        //GameEntry.Event.RemoveEventListener(EventName.LoadingSceneUpdate, OnLoadingProgressChange);
    }
}
