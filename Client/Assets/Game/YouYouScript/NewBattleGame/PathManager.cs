using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    public List<Transform> waypoints;  // 存储路径点

    // 可扩展：运行时管理路径点
    public void AddWaypoint(Transform waypoint)
    {
        if (!waypoints.Contains(waypoint))
        {
            waypoints.Add(waypoint);
        }
    }

    public void RemoveWaypoint(Transform waypoint)
    {
        if (waypoints.Contains(waypoint))
        {
            waypoints.Remove(waypoint);
        }
    }

    // 获取路径点的总数
    public int GetWaypointCount()
    {
        return waypoints.Count;
    }

    // 获取路径点
    public Transform GetWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count)
        {
            return waypoints[index];
        }
        return null;
    }
}