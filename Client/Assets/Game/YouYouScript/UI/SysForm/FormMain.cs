using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Main;
using MessagePack;
using MessagePack.Resolvers;
using Protocols.Player;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;

[System.Serializable]
public class HomePanelButtonData
{
    [HideInInspector] public string BtnType;
    [HideInInspector] public GameObject selectObj;
    [HideInInspector] public GameObject panelObj;
    [HideInInspector] public ShowType showType;
    public GameObject btn;
}

// "主"界面
public class FormMain : UIFormBase
{
    private bool isLoadingPanel = false;
    
    [SerializeField] private List<HomePanelButtonData> btnList = new List<HomePanelButtonData>();
    private Dictionary<string, HomePanelButtonData> btnDic = new Dictionary<string, HomePanelButtonData>();
    protected override void Awake()
    {
        base.Awake();
        GameEntry.Audio.PlayBGM("Home");
        
        foreach (var data in btnList)
        {
            data.BtnType = data.btn.gameObject.name;
            data.btn.GetComponent<Button>().SetButtonClick(() =>
            {
                if(isLoadingPanel) return;
                HandleBtnEvent(data);
            });
            if (data.btn.transform.Find("BarSelect"))
            {
                data.selectObj = data.btn.transform.Find("BarSelect").gameObject;
            }

            btnDic[data.BtnType] = data;
        }
        InitPanelObj(Constants.ItemPath.GoldPanel);
        HandleSelectPanel(btnDic["BattleBtn"]);
    }

    private async UniTask InitPanelObj(string panelPath,HomePanelButtonData data = null,ShowType type = ShowType.Default)
    {
        isLoadingPanel = true;
        PoolObj t = await GameEntry.Pool.GameObjectPool.SpawnAsync(panelPath,transform);
        t.gameObject.SetActive(true);
        if (data != null)
        {
            data.panelObj = t.gameObject;
            data.panelObj.transform.SetSiblingIndex(1);
            data.showType = type;
        }
        isLoadingPanel = false;
    }
    
    
    private void InitPanel(HomePanelButtonData data)
    {
        if (data.BtnType == "ShopBtn") InitPanelObj(Constants.ItemPath.ShopPanel, data);
        else if (data.BtnType == "GuildBtn") InitPanelObj(Constants.ItemPath.GuildPanel, data);
        else if (data.BtnType == "HeroBtn") InitPanelObj(Constants.ItemPath.HeroPanel, data);
        else if (data.BtnType == "LordBtn") InitPanelObj(Constants.ItemPath.LordPanel, data, ShowType.LordPanel);
        else if (data.BtnType == "RankBtn") InitPanelObj(Constants.ItemPath.RankPanel, data);
        else if (data.BtnType == "BattleBtn") InitPanelObj(Constants.ItemPath.MainPanel, data,ShowType.HomePanel);
    }

    private void HandleSelectPanel(HomePanelButtonData data)
    {
        foreach (var pair in btnDic)
        {
            if (pair.Value.selectObj != null)
            {
                if (pair.Value.panelObj != null)
                {
                    pair.Value.panelObj.MSetActive(data.BtnType == pair.Key);
                }
                else
                {
                    if (data.BtnType == pair.Key) InitPanel(pair.Value);
                }
                if (data.BtnType == pair.Key)
                {
                    GoldPanel.Instance.RefreshPos(data.showType);
                }
                pair.Value.selectObj.MSetActive(data.BtnType == pair.Key);
            }
        }
    }

    private void HandleBtnEvent(HomePanelButtonData data)
    {
        HandleSelectPanel(data);
        switch (data.BtnType)
        {
            case "ShopBtn":
                // 处理 ShopBtn 的逻辑
                break;
            case "HeroBtn":
                // 处理 HeroBtn 的逻辑
                break;
            case "BattleBtn":
                // 处理 BattleBtn 的逻辑
                break;
            case "RankBtn":
                // 处理 RankBtn 的逻辑
                break;
            case "LordBtn":
                // 处理 LordBtn 的逻辑
                break;
            case "GuildBtn":
                //GameEntry.Net.Requset.c2s_request_guild_list(1,10);
                GameEntry.Net.Requset.c2s_request_chat(4, "大家好啊！！！！");
                break;
            case "LoginBtn":
                // GameEntry.Net.Requset.c2s_request_update_role_info(new Dictionary<string, string>()
                // {
                //     {nameof(data.UserPassword),"99999"}
                // });
                //GameEntry.SDK.DownloadAvatar("1", null);

                GameEntry.Data.SaveData(true);
                return;
                GameUtil.LogError("111111");
                GameEntry.Audio.PlayBGM("maintheme1");
                GameEntry.Instance.ShowBackGround(BGType.Main, "Assets/Game/Download/Textures/BackGround/Home/home_map_1.png");
                GameEntry.Procedure.ChangeState(ProcedureState.Battle);
                GameEntry.UI.CloseUIForm<FormMain>();
                break;
            default:
                // 处理没有匹配的情况（如果有的话）
                break;
        }
    }
    
    private void Start()
    {

    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
    }
}
