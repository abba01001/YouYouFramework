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
    public GameObject btn;
    public bool HasPanel = false;
}

// "主"界面
public class FormMain : UIFormBase
{
    private bool isLoadingPanel = false;
    
    [SerializeField] private GameObject MoreDetail;
    [SerializeField] private List<HomePanelButtonData> btnList = new List<HomePanelButtonData>();
    private Dictionary<string, HomePanelButtonData> btnDic = new Dictionary<string, HomePanelButtonData>();
    protected override void Awake()
    {
        base.Awake();
        InitPanelObj(Constants.ItemPath.GoldPanel);
        GameEntry.Audio.PlayBGM("Home");
        
        foreach (var data in btnList)
        {
            data.BtnType = data.btn.gameObject.name;
            data.btn.GetComponent<Button>().SetButtonClick(() =>
            {
                if(isLoadingPanel) return;
                HandleSelectPanel(data);
                if (data.BtnType == "ShopBtn")
                {
                    
                }
                else if (data.BtnType == "HeroBtn")
                {
                    
                }
                else if (data.BtnType == "BattleBtn")
                {
                    
                }
                else if (data.BtnType == "RankBtn")
                {
                    
                }
                else if (data.BtnType == "LordBtn")
                {
                    
                }
                else if (data.BtnType == "MoreBtn")
                {
                    MoreDetail.gameObject.MSetActive(!MoreDetail.gameObject.activeSelf);
                }
                else if (data.BtnType == "GuildBtn")
                {
                    //GameEntry.Net.Requset.c2s_request_guild_list(1,10);
                    GameEntry.Net.Requset.c2s_request_chat(4,"大家好啊！！！！");
                }
                else if (data.BtnType == "LoginBtn")
                {
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
                }
            });
            if (data.btn.transform.Find("BarSelect"))
            {
                data.selectObj = data.btn.transform.Find("BarSelect").gameObject;
            }

            if (data.BtnType == "BattleBtn")
            {
                data.panelObj = transform.Find("MainPanel").gameObject;
            }
            btnDic[data.BtnType] = data;
        }
        HandleSelectPanel(btnDic["BattleBtn"]);
    }

    private async UniTask InitPanelObj(string panelPath,HomePanelButtonData data = null)
    {
        isLoadingPanel = true;
        PoolObj t = await GameEntry.Pool.GameObjectPool.SpawnAsync(panelPath,transform);
        t.gameObject.SetActive(true);
        if (data != null)
        {
            data.panelObj = t.gameObject;
        }
        isLoadingPanel = false;
    }
    
    private void InitPanel(HomePanelButtonData data)
    {
        if (data.BtnType == "ShopBtn") InitPanelObj(Constants.ItemPath.ShopPanel, data);
        else if (data.BtnType == "GuildBtn") InitPanelObj(Constants.ItemPath.GuildPanel, data);
        else if (data.BtnType == "HeroBtn") InitPanelObj(Constants.ItemPath.HeroPanel, data);
        else if (data.BtnType == "LordBtn") InitPanelObj(Constants.ItemPath.LordPanel, data);
        else if (data.BtnType == "RankBtn") InitPanelObj(Constants.ItemPath.RankPanel, data);
    }

    private void HandleSelectPanel(HomePanelButtonData data)
    {
        foreach (var pair in btnDic)
        {
            if (pair.Value.selectObj != null)
            {
                if (data.HasPanel)
                {
                    if (pair.Value.panelObj != null)
                    {
                        pair.Value.panelObj.MSetActive(data.BtnType == pair.Key);
                    }
                    else
                    {
                        if (data.BtnType == pair.Key) InitPanel(pair.Value);
                    }
                    pair.Value.selectObj.MSetActive(data.BtnType == pair.Key);
                }
            }
        }
    }
    
    private void Start()
    {
        if (GoldPanel.Instance != null)
        {
            GoldPanel.Instance.RefreshPos(ShowType.HomePanel);
        }
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        if (GoldPanel.Instance != null)
        {
            GoldPanel.Instance.RefreshPos(ShowType.HomePanel);
        }
    }
}
