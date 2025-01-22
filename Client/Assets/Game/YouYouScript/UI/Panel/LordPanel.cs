using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;


[System.Serializable]
public class LordPanelEquip
{
    [HideInInspector] public Image frame;
    [HideInInspector] public Image icon;
    [HideInInspector] public GameObject levelObj;
    [HideInInspector] public Text levelText;
    [HideInInspector] public Text strengthenText;
    public int equipType;
    public Button btn;
}

public enum SelectItemType
{
    Equip,
    Bag
}

public class LordPanel : MonoBehaviour
{    
    [SerializeField] private List<LordPanelEquip> equipBtnList = new List<LordPanelEquip>();
    [SerializeField] private Button EquipBtn;
    [SerializeField] private Button BagBtn;
    [SerializeField] private SkeletonAnimation BigHeroSpine;
    [SerializeField] private SkeletonAnimation SmallHeroSpine;
    public EfficientScrollRect _scrollRect;
    public LordPanelItem itemPrefab;
    private SelectItemType curType = SelectItemType.Bag;
    public bool IsEquipType => curType == SelectItemType.Equip;
    private Dictionary<int, (Sys_EquipEntity,EqiupItemData)> wearEquipList = new Dictionary<int, (Sys_EquipEntity,EqiupItemData)>();

    private int SortingOrder
    {
        get
        {
            int layer = 0;
            if (transform.parent != null && transform.parent.GetComponent<UIFormBase>())
            {
                layer = transform.parent.GetComponent<UIFormBase>().CurrCanvas.sortingOrder;
            }
            return layer;
        }
    }
    private void Awake()
    {


        BagBtn.SetButtonClick(() =>
        {
            HandleSelect(SelectItemType.Bag);
        });
        EquipBtn.SetButtonClick(() =>
        {
            HandleSelect(SelectItemType.Equip);
        });
        itemPrefab.gameObject.MSetActive(false);
        _scrollRect.colSpace = 15;
        _scrollRect.lineSpace = 20;
        _scrollRect.Init(itemPrefab.gameObject,GetData());
        foreach (var item in equipBtnList)
        {
            item.frame = item.btn.transform.Get<Image>("Frame");
            item.icon = item.btn.transform.Get<Image>("Icon");
            item.frame = item.btn.transform.Get<Image>("Frame");
            item.levelObj = item.btn.transform.Get<Transform>("Level").gameObject;
            item.levelText = item.levelObj.transform.Get<Text>("Text");
            item.strengthenText = item.btn.transform.Get<Text>("Text");
            item.btn.SetButtonClick(() =>
            {
                GameEntry.UI.OpenUIForm<FormWearEquip>(item.equipType);
            });
        }
        RefreshEquipInfo();
        HandleSelect(SelectItemType.Bag);
    }

    private void HandleSelect(SelectItemType type)
    {
        curType = type;
        EquipBtn.GetComponent<Image>().SetSpriteByAtlas(Constants.AtlasPath.Common, type == SelectItemType.Equip ? "ty_tab_selected" :"ty_tab_unselected");
        BagBtn.GetComponent<Image>().SetSpriteByAtlas(Constants.AtlasPath.Common, type == SelectItemType.Bag ? "ty_tab_selected" :"ty_tab_unselected");
        _scrollRect.UpdateDatas(GetData(),true);
    }
    
    private void OnEnable()
    {
        GameEntry.Time.Yield(() =>
        {
            BigHeroSpine.transform.parent.GetComponent<SortingGroup>().sortingOrder = SortingOrder + 1;
        });
    }

    private void RefreshEquipInfo()
    {
        wearEquipList.Clear();
        foreach (var data in GameEntry.Data.GetWearEquip())
        {
            Sys_EquipEntity entity = GameEntry.DataTable.Sys_EquipDBModel.GetEntity(data.equipId);
            wearEquipList.TryAdd(entity.Type, (entity,data));
        }
        
        foreach (var item in equipBtnList)
        {
            if (GameEntry.Data.PlayerRoleData.equipLevels.TryGetValue(item.equipType, out int level))
            {
                item.strengthenText.text = $"+{level}";
            }
            
            item.frame.SetSpriteByAtlas(Constants.AtlasPath.LordPanel,$"ty_frame_A1", true);
            if (wearEquipList.TryGetValue(item.equipType, out var value))
            {
                Sys_EquipEntity entity = value.Item1;
                EqiupItemData data = value.Item2;
                
                item.levelObj.gameObject.MSetActive(true);
                item.levelText.text = entity.Stage.ToString();
                
                item.icon.gameObject.MSetActive(true);
                item.icon.SetSpriteByAtlas(Constants.AtlasPath.Equip,$"icon_zhuangbei0{entity.Type}_{entity.Stage}", true);
                item.frame.SetSpriteByAtlas(Constants.AtlasPath.LordPanel,$"ty_frame_A{data.quality}", true);
            }
            else
            {
                item.icon.gameObject.MSetActive(false);
                item.levelObj.gameObject.MSetActive(false);
            }
        }
    }
    
    object[] GetData()
    {
        if (curType == SelectItemType.Equip)
        {
            List<EqiupItemData> infos = GameEntry.Data.PlayerRoleData.equipWareHouse;
            return infos.ToArray();
        }
        else
        {
            List<BagItemData> infos = GameEntry.Data.PlayerRoleData.bagWareHouse;
            return infos.ToArray();
        }
    }
}