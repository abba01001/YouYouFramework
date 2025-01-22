using System;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

// "登录"界面
public class FormItemInfo : UIFormBase
{
    private int curSelectEquipType;
    [SerializeField] private Button closeBtn;
    protected override void Awake()
    {
        base.Awake();
        closeBtn.SetButtonClick(() =>
        {
            GameEntry.UI.CloseUIForm<FormItemInfo>();
        });
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        //GameEntry.Event.AddEventListener(Constants.EventName.GetSuspendReward,HandleGetReward);

    }

    protected override void OnShow()
    {
        base.OnShow();
        if (userData != null)
        {
            var tupleValue = (ValueTuple<SelectItemType, object>)userData;
            if (tupleValue.Item1 == SelectItemType.Bag)
            {
                BagItemData data = tupleValue.Item2 as BagItemData;
                GameUtil.LogError("背包====",data.itemId);
            }
            else
            {
                EqiupItemData data = tupleValue.Item2 as EqiupItemData;
                GameUtil.LogError("装备====",data.equipId);
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        //GameEntry.Event.RemoveEventListener(Constants.EventName.GetSuspendReward,HandleGetReward);
    }
}
