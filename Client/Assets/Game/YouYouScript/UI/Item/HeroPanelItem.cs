using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class HeroPanelItem : ScrollItem
{
    [SerializeField] private Image HeroIcon;
    private Sys_ModelEntity curEntity;
    public override void OnDataUpdate(object data, int index)
    {
        base.OnDataUpdate(data, index);
        Sys_ModelEntity entity = data as Sys_ModelEntity;

        if (curEntity != entity)
        {
            curEntity = entity;
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
        HeroIcon.SetSpriteByAtlas(Constants.AtlasPath.HeroPanel, curEntity.HeroPanelIcon, true);

    }
}
