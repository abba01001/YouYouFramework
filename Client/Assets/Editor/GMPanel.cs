using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sirenix.Utilities.Editor;
using Watermelon;

public class GMPanel : OdinEditorWindow
{
    private static GMPanel window;

    [MenuItem("Tools/GM Panel #Q")]
    private static void OpenWindow()
    {
        if (window == null || !window.hasFocus)
        {
            window = GetWindow<GMPanel>();
            window.Show();
        }
        else
        {
            window.Close();
            window = null; // 重置窗口引用
        }
    }

    // 按钮配置列表
    private List<ButtonConfig> buttonConfigs;

    [TabGroup("基本设置")] public float basicValue = 10;

    // 玩家信息区域
    [TabGroup("基本设置")] [LabelText("玩家数据管理")] [GUIColor(0.5f, 0.8f, 0.2f)]
    public PlayerData playerData = new PlayerData();

    [TabGroup("基本设置")]
    // 批量操作区域
    [FoldoutGroup("批量操作", true)]
    [Title("批量金币修改")]
    private void AddCoinsToPlayers()
    {
        // foreach (var player in players)
        // {
        //     player.Coins += 100;
        //     Debug.Log($"给 {player.Name} 增加了100金币, 当前金币: {player.Coins}");
        // }

        CurrencyController.Set(CurrencyType.Coins, 10000);
    }

    // 玩家列表，用于批量修改
    [FoldoutGroup("批量操作", false)] public PlayerData[] players;

    // 枚举列表
    [FoldoutGroup("枚举示例", false)] [LabelText("选择枚举列表")]
    public ControlEnum selectedEnum = ControlEnum.None;

    [FoldoutGroup("滑动条示例", false)] [LabelText("滑动条")] [Slider(0, 100)] // 0 到 100 的滑动条
    public float sliderValue = 50; // 初始值是 50

    [FoldoutGroup("开关示例", false)] [LabelText("开关")] [ToggleLeft()]
    public bool enableFeature = false;

    
    
    
    [FoldoutGroup("货币列表")] [LabelText("下拉栏")] [ValueDropdown("GetOptions")]
    public string selectedOption;

    [FoldoutGroup("货币列表")] [LabelText("输入框")]
    public int textInput = 0;

    [FoldoutGroup("货币列表")]
    [Sirenix.OdinInspector.Button("点击按钮")]
    private void OnButtonClick()
    {
        Debug.Log("按钮被点击了！");
    }

    // 获取枚举的中文描述
    private string GetEnumDescription(CurrencyType currency)
    {
        var field = currency.GetType().GetField(currency.ToString());
        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute != null ? attribute.Description : currency.ToString();  // 如果没有描述，返回枚举的名称
    }
    
    private IEnumerable GetOptions()
    {
        // 获取枚举所有项并转换为字符串数组
        return Enum.GetValues(typeof(CurrencyType))
            .Cast<CurrencyType>()
            .Select(e => new ValueDropdownItem($"{GetEnumDescription(e)}|{e}", e.ToString()))  // 显示中文+英文
            .ToArray();
    }

    [LabelText("用户ID")] [FoldoutGroup("多行文本框示例", false)]
    public string longText1 = "这里可以输入很多文字...";
    
    [LabelText("邮件信息")] [FoldoutGroup("多行文本框示例", false)] [TextArea]
    public string longText2 = "这里可以输入很多文字...";

    [FoldoutGroup("多行文本框示例")]
    [Sirenix.OdinInspector.Button("点击按钮")]
    private void OnButtonClick1()
    {
        Debug.Log("按钮被点击了！");
    }

    [TabGroup("高级设置")] public float advancedValue = 20;

    protected override void OnImGUI()
    {
        base.OnImGUI();

        InitButtons();
        InitCurrency();
    }

    private void InitCurrency()
    {
        
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    //初始化按钮
    private void InitButtons()
    {
        // 初始化按钮配置（这里可以动态加载配置）
        if (buttonConfigs == null || buttonConfigs.Count == 0)
        {
            buttonConfigs = new List<ButtonConfig>
            {
                new ButtonConfig("批量增加金币", AddCoinsToPlayers, GetButtonStyle(ButtonStyleType.Primary),
                    new Rect(100, 400, 200, 50)),
                new ButtonConfig("其他功能按钮", OtherFunction, GetButtonStyle(ButtonStyleType.Secondary),
                    new Rect(300, 400, 200, 50))
            };
        }

        foreach (var buttonConfig in buttonConfigs)
        {
            if (GUI.Button(buttonConfig.Position, buttonConfig.Text, buttonConfig.Style))
            {
                buttonConfig.OnClick?.Invoke();
            }
        }
    }

    // 获取按钮样式
    private GUIStyle GetButtonStyle(ButtonStyleType styleType)
    {
        switch (styleType)
        {
            case ButtonStyleType.Primary:
                return new GUIStyle(GUI.skin.button)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    richText = true,
                    alignment = TextAnchor.MiddleCenter
                };
            case ButtonStyleType.Secondary:
                return new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Italic,
                    richText = true,
                    alignment = TextAnchor.MiddleCenter
                };
            default:
                return new GUIStyle(GUI.skin.button);
        }
    }

    // 其他功能按钮的操作
    private void OtherFunction()
    {
        Debug.Log("执行其他功能...");
    }
}


// 控制枚举：你可以在这里扩展不同的控件类型
public enum ControlEnum
{
    None,
    Option1,
    Option2,
    Option3
}

// 按钮配置类
public class ButtonConfig
{
    public string Text { get; }
    public System.Action OnClick { get; }
    public GUIStyle Style { get; }
    public Rect Position { get; }

    public ButtonConfig(string text, System.Action onClick, GUIStyle style, Rect position)
    {
        Text = text;
        OnClick = onClick;
        Style = style;
        Position = position;
    }
}

// 按钮样式类型枚举
public enum ButtonStyleType
{
    Primary,
    Secondary
}

// 玩家数据结构
[System.Serializable]
public class PlayerData
{
    public string Name;
    public int Level;
    public int Coins;

    public PlayerData(string name = "玩家1", int level = 1, int coins = 0)
    {
        Name = name;
        Level = level;
        Coins = coins;
    }
}