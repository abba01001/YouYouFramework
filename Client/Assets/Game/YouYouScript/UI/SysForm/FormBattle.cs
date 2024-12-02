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
    public static CardPlacement CardPlacement;
    [SerializeField] private Button exitBtn;
    [SerializeField] private Button testBtn;
    protected override void Awake()
    {
        base.Awake();
        exitBtn.SetButtonClick(()=>{
            GameEntry.Procedure.ChangeState(ProcedureState.Game);
        });
        testBtn.SetButtonClick(() =>
        {
            GameEntry.UI.OpenUIForm<FormMap>();
        });
        CardPlacement = transform.Get<CardPlacement>("BattleCardPanel");
        CardPlacement.Init(this.transform);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEntry.Event.AddEventListener(Constants.EventName.InitCardObj, InitCards);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        GameEntry.Event.RemoveEventListener(Constants.EventName.InitCardObj, InitCards);
    }

    private async void InitCards(object userdata)
    {
        List<BaseCard> list = new List<BaseCard>();
        for (int i = 0; i < 3; i++)
        {
            PoolObj poolObj = await GameEntry.Pool.GameObjectPool.SpawnAsync(Constants.ItemPath.CardObj,CardPlacement.transform);
            list.Add(poolObj.GetComponent<BaseCard>());
        }
        CardPlacement.StartPlay(list);
    }
}
