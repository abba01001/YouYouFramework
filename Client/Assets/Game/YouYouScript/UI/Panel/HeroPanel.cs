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
    private void Awake()
    {
        List<object> datas = new List<object> { "开始游戏", "111", "2222", "4444" };
        for (int i = 0; i < 40; i++)
        {
            datas.Add($"新数据{i}");
        }
        object[] dataArray = datas.ToArray();
        _scrollRect.Init(itemPrefab.gameObject,dataArray);
    }
}