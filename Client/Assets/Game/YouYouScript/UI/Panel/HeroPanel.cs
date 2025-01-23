using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;

public class HeroPanel : MonoBehaviour
{
    public EfficientScrollRect _scrollRect;
    public HeroPanelItem itemPrefab;
    public Button heroBtn;
    public Button bookBtn;
    private void Awake()
    {
        itemPrefab.gameObject.MSetActive(false);
        heroBtn.SetButtonClick(() =>
        {
            
        });
        bookBtn.SetButtonClick(() =>
        {
            
        });
        _scrollRect.Init(itemPrefab.gameObject,GetData());
    }

    private void OnEnable()
    {

    }

    object[] GetData()
    {
        List<Sys_ModelEntity> list = new List<Sys_ModelEntity>();
        foreach (var pair in GameEntry.DataTable.Sys_ModelDBModel.IdByDic)
        {
            if (pair.Value.InHeroPanel == 1)
            {
                Sys_ModelEntity entity = pair.Value;
                list.Add(pair.Value);
            }
        }

        object[] data = new object[list.Count];
        int index = 0;
        foreach (var VARIABLE in list)
        {
            data[index] = VARIABLE;
            index++;
        }
        return data;
    }
}