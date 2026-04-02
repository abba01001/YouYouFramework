using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



// "主"界面
public class FormMain : UIFormBase
{
    [SerializeField] private Button testBtn;
    [SerializeField] private Button testBt1;
    [SerializeField] private Button testBt2;
    protected override async UniTask Awake()
    {
        await base.Awake();
        Constants.IsEntryFormMain = true;
        testBtn.SetButtonClick(() =>
        {
            GameEntry.Data.AddMoney(500);
        });
        testBt1.SetButtonClick(() =>
        {
            PlayerPrefs.DeleteAll();
            Application.Quit(); 
        });
        testBt2.SetButtonClick(() =>
        {
        });
    }

    protected override void OnShow()
    {
        base.OnShow();
    }
    
    public Text collectedMoney;
    public GameObject dragToMoveWindow;
    public GameObject settingsPanel;

    private void Start()
    {
        if(PlayerPrefs.HasKey("DragWindow")) Destroy(dragToMoveWindow);
        collectedMoney.text = GameEntry.Data.Coin.ToString();

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEntry.Event.AddEventListener(Constants.EventName.SetMoneyText,SetMoneyText);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        GameEntry.Event.RemoveEventListener(Constants.EventName.SetMoneyText,SetMoneyText);
    }
    
    public void SetMoneyText(object userdata)
    {
        collectedMoney.text = "$" + ((int)userdata).ToString();
    }

    public void OpenSettingsWindow()
    {
        PlayerPrefs.DeleteAll();
        settingsPanel.SetActive(true);
    }
}
