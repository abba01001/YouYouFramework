using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class LordPanelItem : ScrollItem
{
    [SerializeField] private LordPanel Parent;
    [SerializeField] private Image Frame;
    [SerializeField] private Image Icon;
    [SerializeField] private GameObject LevelObj;
    [SerializeField] private Text LevelText;
    [SerializeField] private Text NumText;
    private object curData;
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

    private void RefreshData()
    {
        LevelObj.gameObject.MSetActive(Parent.IsEquipType);
        NumText.text = "";
        if (Parent.IsEquipType)
        {
            EqiupItemData data = curData as EqiupItemData;
            Sys_EquipEntity entity = GameEntry.DataTable.Sys_EquipDBModel.GetEntity(data.equipId);
            if (entity != null)
            {
                LevelText.text = $"{entity.Stage}阶";
                Icon.SetSpriteByAtlas(Constants.AtlasPath.Equip,$"icon_zhuangbei0{entity.Type}_{entity.Stage}", true);
                Frame.SetSpriteByAtlas(Constants.AtlasPath.LordPanel,$"ty_frame_A{data.quality}", true);
            }
        }
        else
        {
            BagItemData data = curData as BagItemData;
            Sys_ItemEntity entity = GameEntry.DataTable.Sys_ItemDBModel.GetEntity(data.itemId);
            NumText.text = data.itemCount.ToString();
            if (entity != null)
            {
                //Icon.SetSpriteByAtlas(Constants.AtlasPath.Daoju,$"icon_zhuangbei0{entity.Type}_{entity.Stage}", true);
            }
            //NumText.text = da
        }
        //HeroIcon.SetSpriteByAtlas(Constants.AtlasPath.HeroPanel, curEntity.HeroPanelIcon, true);

    }
}
