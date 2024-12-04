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
    [SerializeField] private Button exitBtn;
    [SerializeField] private Button testBtn;
    [SerializeField] private Button callBtn;
    protected override void Awake()
    {
        base.Awake();
        // exitBtn.SetButtonClick(()=>{
        //     GameEntry.Procedure.ChangeState(ProcedureState.Game);
        // });
        // testBtn.SetButtonClick(() =>
        // {
        //     GameEntry.UI.OpenUIForm<FormMap>();
        // });
        callBtn.SetButtonClick(() =>
        {
            BattleGridManager.Instance.CallHero();
        });
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }

}
