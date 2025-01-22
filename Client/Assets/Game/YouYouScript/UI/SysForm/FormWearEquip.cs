using UnityEngine;
using UnityEngine.UI;
using YouYou;

// "登录"界面
public class FormWearEquip : UIFormBase
{
    private int curSelectEquipType;
    [SerializeField] private Button closeBtn;
    protected override void Awake()
    {
        base.Awake();
        closeBtn.SetButtonClick(() =>
        {
            GameEntry.UI.CloseUIForm<FormWearEquip>();
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
            curSelectEquipType = (int) userData;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        //GameEntry.Event.RemoveEventListener(Constants.EventName.GetSuspendReward,HandleGetReward);
    }
}
