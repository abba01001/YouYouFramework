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
    }

    protected override void OnShow()
    {
        base.OnShow();

        InitPlayer();
        
    }
    
    async UniTask InitPlayer()
    {
        PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/Role/Player.prefab");
        obj.GetComponent<PlayerController>().joystick = joystick;
        obj.gameObject.MSetActive(true);
        Scene targetScene = SceneManager.GetSceneByName("Main");
        if (targetScene.IsValid())
        {
            SceneManager.MoveGameObjectToScene(obj.gameObject, targetScene);
        }
        GameEntry.Event.Dispatch(Constants.EventName.UpdateFoodPlayerCarry,obj.GetComponent<PlayerManager>().maxFoodPlayerCarry);
        GameEntry.Instance.PlayerController = obj.GetComponent<PlayerController>();

        BuildingSystem.Instance.Init();
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
            BuildingSystem.Instance.Test();
        }
    }

    private void Start()
    {
        if(PlayerPrefs.HasKey("DragWindow")) Destroy(dragToMoveWindow);

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
