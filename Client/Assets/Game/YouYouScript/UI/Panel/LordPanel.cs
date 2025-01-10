using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;

public class LordPanel : MonoBehaviour
{
    private enum SelectItemType
    {
        Equip,
        Bag
    }
    public EfficientScrollRect _scrollRect;
    public LordPanelItem itemPrefab;
    private SelectItemType curType = SelectItemType.Equip;

    public bool IsEquipType => curType == SelectItemType.Equip;

    private void Awake()
    {
        itemPrefab.gameObject.MSetActive(false);
        _scrollRect.Init(itemPrefab.gameObject,GetData());
    }
    
    private void OnEnable()
    {

    }
    
    object[] GetData()
    {
        if (curType == SelectItemType.Equip)
        {
            List<EqiupItemData> infos = GameEntry.Data.PlayerRoleData.equipWareHouse;//new List<EqiupItemData>();
            // for (int i = 0; i < 20; i++)
            // {
            //     infos.Add(new EqiupItemData()
            //     {
            //         equipId = GameUtil.RandomRange(101,120),
            //         quality = GameUtil.RandomRange(1,8)
            //     });
            // }
            // infos.Sort((a, b) =>
            // {
            //     int equipIdComparison = b.equipId.CompareTo(a.equipId);  // equipId 由大到小
            //     if (equipIdComparison != 0) return equipIdComparison;
            //     return b.quality.CompareTo(a.quality);  // 如果 equipId 相同，按 quality 由大到小
            // });
            // GameEntry.Data.PlayerRoleData.equipWareHouse = infos;
            // GameEntry.Data.SaveData(true);
            return infos.ToArray();
        }
        else
        {
            List<BagItemData> infos = new List<BagItemData>();
            return infos.ToArray();
        }
    }
}