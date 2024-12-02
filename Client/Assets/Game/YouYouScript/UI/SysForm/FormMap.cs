using UnityEngine;
using UnityEngine.UI;
using YouYou;


/// <summary>
/// "战斗"界面
/// </summary>
public class FormMap : UIFormBase
{
    [SerializeField] private Button exitBtn;

    protected override void Awake()
    {
        base.Awake();
        exitBtn.SetButtonClick(Close);
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
