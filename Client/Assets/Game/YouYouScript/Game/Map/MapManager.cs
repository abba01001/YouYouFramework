#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using YouYou;

public class MapManager : MonoBehaviour
{
    public Transform Player;
    private Transform Root;
    private Sys_BuildingsDBModel Sys_BuildingsDBModel;
    private Dictionary<int, GameObject> RegionMap = new Dictionary<int, GameObject>();
    private void Start()
    {
        Root = new GameObject("Root").transform;
        InitRegion();
        InitPlayer();
        InitConfig();
        InitBuilding();
    }

    private void InitConfig()
    {
        
        // 文件路径
        string filePath = "Assets/Game/Download/DataTable/Sys_Buildings.bytes";

        // 读取文件内容为字节数组
        byte[] fileData = File.ReadAllBytes(filePath);

        // 创建 MMO_MemoryStream 实例（假设它有类似于 MemoryStream 的构造函数）
        MMO_MemoryStream ms = new MMO_MemoryStream(fileData);

        Sys_BuildingsDBModel =  new Sys_BuildingsDBModel();
        Sys_BuildingsDBModel.TestLoad(ms);

    }
    
    private void InitBuilding()
    {
        foreach (var kv in Sys_BuildingsDBModel.IdByDic)
        {
            CreateBuilding(kv.Value);
        }
    }
    
    private void CreateBuilding(Sys_BuildingsEntity entity)
    {
        if (entity == null) return;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Game/Download/Prefab/Regions/{entity.BuildingName}.prefab");
        if (prefab == null)
        {
            GameUtil.LogError("未找到实例==>",entity.BuildingName);
        }
        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity,RegionMap[entity.RegionId].transform);
        BuildingBase building = obj.GetComponent<BuildingBase>();
        building.Init(entity);
        obj.gameObject.MSetActive(true);
        obj.transform.position = GameUtil.ParseCoordinates(entity.Position);
        obj.transform.rotation = Quaternion.Euler(GameUtil.ParseCoordinates(entity.Rotation));
    }
    
    private void InitRegion()
    {
        for (int i = 1; i <= 2; i++)
        {
            string path = $"Assets/Game/Download/Prefab/Regions/Area{i}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity,Root);
            RegionMap.Add(i,obj);
        }
    }

    private void InitPlayer()
    {
        Player.gameObject.MSetActive(true);
    }
    
}
#endif


