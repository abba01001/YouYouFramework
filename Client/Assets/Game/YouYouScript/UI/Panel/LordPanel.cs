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
    public HeroPanelItem itemPrefab;
    private SelectItemType curType = SelectItemType.Equip;
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
        Dictionary<int, int> infos = new Dictionary<int, int>();
        infos = curType == SelectItemType.Equip ? GameEntry.Data.PlayerRoleData.equipWareHouse : GameEntry.Data.PlayerRoleData.bagWareHouse;
        
        object[] data = new object[infos.Count];
        int index = 0;
        foreach (var VARIABLE in infos)
        {
            data[index] = VARIABLE;
            index++;
        }
        return data;
    }
}