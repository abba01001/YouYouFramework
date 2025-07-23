using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapManager))]
public class MapManagerEditor : Editor
{
    private bool connectStartToEnd = false; // 控制是否连接路径的起点与终点

    // 在面板中显示自定义按钮
    public override void OnInspectorGUI()
    {
        MapManager pathManager = (MapManager)target;

        // 显示默认的 Inspector 内容
        DrawDefaultInspector();

    }

    void OnSceneGUI()
    {
        MapManager pathManager = (MapManager)target;
    }
}
