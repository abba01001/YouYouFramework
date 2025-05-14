using System;
using System.Collections;
using System.Collections.Generic;
using Protocols;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class MainChallengeItem : ScrollItem
{
    [SerializeField] private LordPanel Parent;
    [SerializeField] private TMP_InputField content;
    [SerializeField] private RectTransform bgRect;
    [SerializeField] private Text LevelNum;
    [SerializeField] public Button _button;
    [SerializeField] private Image bgImage;
    [SerializeField] private GameObject model;
    [SerializeField] private GameObject enemyModel;
    [SerializeField] private GameObject tipObj;
    private object curData;
    private int selectIndex = -1;
    private void Awake()
    {
        bgImage.color = Color.clear;
        tipObj.MSetActive(false);
    }

    public override void OnDataUpdate(object data, int index)
    {
        base.OnDataUpdate(data, index);

        if (curData != data)
        {
            curData = data;
            RefreshData();
        }
        // Sprite t = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Download/Textures/HeroPanel/yxkp_E_silingwuzhu.png");
        // if (t == null)
        // {
        //     Debug.LogError("加载的资源不是一个有效的 Sprite 或路径错误！");
        // }
        // else
        // {
        //     HeroIcon.sprite = t;
        // }
        //HeroIcon.SetImage(Constants.TexturePath.HeroPanel,entity.HeroPanelIcon,true);
    }

    public void RefreshSelectIndex(bool bo)
    {
        selectIndex = bo ? Index : -1;
    }
    
    public void SelectObj(bool bo)
    {
        bgImage.color = bo ? Color.white : Color.clear;
        model.gameObject.MSetActive(bo);
    }
    
    private void RefreshData()
    {
        LevelModel data = curData as LevelModel;
        if (data.levelNum == 0 || data.levelNum == 6666)
        {
            transform.localScale = Vector3.zero;
            return;
        }
        SelectObj(selectIndex == Index);
        LevelNum.text = data.levelNum.ToString();
        // GameEntry.Time.CreateTimer(this,0.02f, AdaptTextBg);
        //content.text = data.Message;
        // LevelObj.gameObject.MSetActive(Parent.IsEquipType);
        // NumText.text = "";
        // if (Parent.IsEquipType)
        // {
        //     EqiupItemData data = curData as EqiupItemData;
        //     Sys_EquipEntity entity = GameEntry.DataTable.Sys_EquipDBModel.GetEntity(data.equipId);
        //     if (entity != null)
        //     {
        //         LevelText.text = $"{entity.Stage}阶";
        //         Icon.SetSpriteByAtlas(Constants.AtlasPath.Equip,$"icon_zhuangbei0{entity.Type}_{entity.Stage}", true);
        //         Frame.SetSpriteByAtlas(Constants.AtlasPath.LordPanel,$"ty_frame_A{data.quality}", true);
        //     }
        // }
        // else
        // {
        //     BagItemData data = curData as BagItemData;
        //     Sys_ItemEntity entity = GameEntry.DataTable.Sys_ItemDBModel.GetEntity(data.itemId);
        //     NumText.text = data.itemCount.ToString();
        //     if (entity != null)
        //     {
        //         //Icon.SetSpriteByAtlas(Constants.AtlasPath.Daoju,$"icon_zhuangbei0{entity.Type}_{entity.Stage}", true);
        //     }
        //     //NumText.text = da
        // }
        //HeroIcon.SetSpriteByAtlas(Constants.AtlasPath.HeroPanel, curEntity.HeroPanelIcon, true);
    }

    private void AdaptTextBg()
    {
        float width = 0;
        float height = 0;
        if (content.preferredWidth >= 100)
        {
            width = 114 + content.preferredWidth - 100;
        }
        bgRect.sizeDelta = new Vector2(Mathf.Clamp(width, 114, 420), Mathf.Clamp(height, 60, 75));
        //content.preferredWidth
    }
}
