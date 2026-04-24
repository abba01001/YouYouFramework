using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CommonEditor : OdinMenuEditorWindow
{
    [MenuItem("工具类/编辑器工具",false,-100)]
    private static void OpenCommonEditor()
    {
        var window = GetWindow<CommonEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 500);
    }
    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true);
        //参数设置
        tree.AddAssetAtPath("参数相关", "Game/Scripts/FrameworkScripts/ScriptableObject/ParamsSettings.asset");
        
        //宏设置
        tree.AddAssetAtPath("宏定义相关", "Game/Scripts/FrameworkScripts/ScriptableObject/MacroSettings.asset");

        //AssetBundle打包管理
        tree.AddAssetAtPath("打包相关", "Game/Scripts/FrameworkScripts/ScriptableObject/AssetBundleSettings.asset");

        //通用工具
        tree.AddAssetAtPath("通用工具", "Game/Scripts/FrameworkScripts/ScriptableObject/CommonToolsSettings.asset");
        return tree;
    }
}
