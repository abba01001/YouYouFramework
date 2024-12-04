using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathManager))]
public class PathManagerEditor : Editor
{
    private bool connectStartToEnd = false; // 控制是否连接路径的起点与终点

    // 在面板中显示自定义按钮
    public override void OnInspectorGUI()
    {
        PathManager pathManager = (PathManager)target;

        // 显示默认的 Inspector 内容
        DrawDefaultInspector();

        // 添加按钮的操作
        if (GUILayout.Button("Add Waypoint"))
        {
            Undo.RecordObject(pathManager, "Add Waypoint");

            // 创建一个新的路径点并将其添加到列表
            GameObject newWaypoint = new GameObject("Waypoint");

            // 确定偏移量，这里我们使用 5 的偏移，向右（x 轴）和向上（y 轴）
            Vector3 offset = new Vector3(5f, 5f, 0f); // 2D游戏偏移量，改变 x 和 y

            // 如果有已有路径点，偏移上一个路径点；否则，放置在原点
            if (pathManager.waypoints.Count > 0)
            {
                newWaypoint.transform.position = pathManager.waypoints[pathManager.waypoints.Count - 1].position + offset;
            }
            else
            {
                newWaypoint.transform.position = Vector3.zero; // 如果没有路径点，放置在原点
            }

            pathManager.AddWaypoint(newWaypoint.transform);
        }

        // 可视化路径点的数量
        EditorGUILayout.LabelField("Waypoint Count", pathManager.waypoints.Count.ToString());

        // 绘制是否连接起点和终点的选项
        connectStartToEnd = EditorGUILayout.Toggle("Connect Start to End", connectStartToEnd);

        // 每次修改时，强制刷新 SceneView
        if (GUI.changed)
        {
            SceneView.RepaintAll();
        }

        // 添加保存路径按钮
        if (GUILayout.Button("Save Path"))
        {
            // 创建一个新的空 GameObject 来保存路径点
            GameObject pathContainer = new GameObject("PathContainer");

            // 临时存储新的路径点列表
            List<Transform> newWaypoints = new List<Transform>();

            // 将路径点复制到新的物体下
            if (pathManager.waypoints.Count > 0)
            {
                foreach (var waypoint in pathManager.waypoints)
                {
                    if (waypoint != null)
                    {
                        // 复制路径节点
                        GameObject waypointCopy = Instantiate(waypoint.gameObject);
                        waypointCopy.name = waypoint.name; // 复制节点的名称
                        waypointCopy.transform.position = waypoint.position; // 保持路径点的位置
                        waypointCopy.transform.SetParent(pathContainer.transform); // 将复制的路径节点作为子物体

                        // 添加到新的路径点列表
                        newWaypoints.Add(waypointCopy.transform);
                    }
                }

                // 清空原有的路径点列表并替换为新的路径点
                pathManager.waypoints.Clear();
                pathManager.waypoints.AddRange(newWaypoints);

                Debug.Log("Path saved to new GameObject: " + pathContainer.name);
            }
            else
            {
                Debug.LogWarning("No waypoints to save.");
            }
        }
    }

    void OnSceneGUI()
    {
        PathManager pathManager = (PathManager)target;

        // 确保 pathManager 和 waypoints 已经初始化
        if (pathManager == null || pathManager.waypoints == null || pathManager.waypoints.Count == 0)
        {
            return; // 如果 pathManager 或 waypoints 为空，则退出
        }

        // 绘制路径点和路径
        for (int i = 0; i < pathManager.waypoints.Count; i++)
        {
            Transform waypoint = pathManager.waypoints[i];
            if (waypoint == null) return;
            // 绘制路径点
            Handles.color = Color.green;
            Handles.SphereHandleCap(0, waypoint.position, Quaternion.identity, 0.2f, EventType.Repaint);

            // 可编辑路径点位置
            waypoint.position = Handles.PositionHandle(waypoint.position, Quaternion.identity);

            // 删除路径点的按钮
            if (Handles.Button(waypoint.position + Vector3.up * 0.5f, Quaternion.identity, 0.2f, 0.2f,
                    Handles.SphereHandleCap))
            {
                Undo.RecordObject(pathManager, "Remove Waypoint");
                pathManager.RemoveWaypoint(waypoint); // 删除路径点
            }
        }

        // 如果选中了“连接起点和终点”，则绘制一条线连接路径的起点和终点
        if (connectStartToEnd && pathManager.waypoints.Count > 1)
        {
            Handles.color = Color.red;
            // 连接路径的第一个和最后一个路径点
            Handles.DrawLine(pathManager.waypoints[0].position, pathManager.waypoints[pathManager.waypoints.Count - 1].position);
        }

        // 绘制路径点之间的线
        for (int i = 0; i < pathManager.waypoints.Count - 1; i++)
        {
            Handles.color = Color.yellow;
            Handles.DrawLine(pathManager.waypoints[i].position, pathManager.waypoints[i + 1].position); // 绘制路径连线
        }
    }
}
