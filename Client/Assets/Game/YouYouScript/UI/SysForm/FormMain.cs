using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YouYou;


// "主"界面
public class FormMain : UIFormBase
{
    protected override void Awake()
    {
        base.Awake();
        Constants.IsEntryFormMain = true;
        // testBtn.SetButtonClick(() =>
        // {
        //     GameEntry.Data.AddMoney(500);
        // });
        // testBt1.SetButtonClick(() =>
        // {
        //     PlayerPrefs.DeleteAll();
        //     Application.Quit(); 
        // });
        // testBt2.SetButtonClick(() =>
        // {
        //     GameEntry.UI.OpenUIForm<FormUpgrade>();
        // });
    }

    protected override void OnShow()
    {
        base.OnShow();
        BuildingSystem.Instance.PlayerController.joystick = joystick;
    }
    
    async UniTask InitPlayer()
    {
        PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/Role/Player.prefab");
        obj.GetComponent<PlayerController>().joystick = joystick;
        obj.gameObject.MSetActive(true);
        Scene targetScene = SceneManager.GetSceneByName("Main");
        // if (targetScene.IsValid())
        // {
        //     SceneManager.MoveGameObjectToScene(obj.gameObject, targetScene);
        // }
        GameEntry.Event.Dispatch(Constants.EventName.UpdateFoodPlayerCarry,obj.GetComponent<PlayerManager>().maxFoodPlayerCarry);
        BuildingSystem.Instance.PlayerController = obj.GetComponent<PlayerController>();

    }
    
    public Joystick joystick;
    public Text collectedMoney;
    public GameObject dragToMoveWindow;
    public GameObject settingsPanel;
    private void Update()
    {
        if (Input.GetMouseButton(0) && dragToMoveWindow)
        {
            PlayerPrefs.SetString("DragWindow","");
            Destroy(dragToMoveWindow);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            // BuildingSystem.Instance.Test();
            // GameEntry.Data.AddMoney(2000);
        }
    }

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
        MyAdManager.Instance.ShowInterstitialAd();
        settingsPanel.SetActive(true);
    }
}
