using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;


public enum ShowType
{
    HomePanel
}

[System.Serializable]
public class GoldPanelButtonData
{
    [HideInInspector] public string HuoBiType;
    public GameObject btn;
    [HideInInspector] public Text tx;
}

public class GoldPanel : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField] private RectTransform btnParentRect;
    [SerializeField] private List<GoldPanelButtonData> btnList = new List<GoldPanelButtonData>();
    private Dictionary<string, GoldPanelButtonData> btnDic = new Dictionary<string, GoldPanelButtonData>();
    public static GoldPanel Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (var data in btnList)
        {
            data.HuoBiType = data.btn.gameObject.name;
            data.tx = data.btn.Get<Text>("Text");
            GameEntry.Data.PlayerRoleData.roleAttr.TryGetValue(data.HuoBiType, out int value);
            data.tx.text = $"{value}";
            data.btn.GetComponent<Button>().SetButtonClick(() => { GameUtil.LogError($"弹出HuoBi{data.HuoBiType}Btn"); });
            btnDic[data.HuoBiType] = data;
        }
    }

    public void RefreshPos(ShowType showType)
    {
        if (showType == ShowType.HomePanel)
        {
            btnParentRect.anchoredPosition = new Vector2(150, -213);
        }
    }
}