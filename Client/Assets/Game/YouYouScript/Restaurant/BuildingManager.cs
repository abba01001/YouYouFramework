using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public class BuildingManager
{
    private static BuildingManager _instance;
    private BuildingManager() { }

    public static BuildingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BuildingManager();
            }
            return _instance;
        }
    }

    public void Update()
    {
        
    }

    // 使用 HashSet 来避免重复的区域
    private Dictionary<int, List<Sys_BuildingsEntity>> Regions = new Dictionary<int, List<Sys_BuildingsEntity>>();
    private List<BuildingBase> Buildings = new List<BuildingBase>();
    private Dictionary<int,GameObject> RegionMap = new Dictionary<int,GameObject>();
    // 将 UnlockRegionIds 改为成员变量
    private HashSet<int> UnlockRegionIds = new HashSet<int> { 1 };  // 初始化时就加入默认的区域 1

    public async UniTask Init()
    {
        InitConfig();
        await GenRegionMap();  // 生成地图
        await GenInitBuildings();  // 生成初始建筑
        await GenDynamicBuildings();  // 生成动态建筑
        await CustomerManager.Instance.Init(); //生成顾客
        await HelperManager.Instance.Init(); //生成协助者
        await GenPlayer();//生成玩家
    }

    private async UniTask GenPlayer()
    {
        
    }

    // 生成区域地图
    private async UniTask GenRegionMap()
    {
        foreach (var id in UnlockRegionIds)
        {
            if(RegionMap.ContainsKey(id)) continue;
            PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/Regions/Region{id}");
            RegionMap.Add(id,obj.gameObject);
        }
    }

    // 生成初始化建筑
    private async UniTask GenInitBuildings()
    {
        foreach (var kv in Regions)
        {
            if (!UnlockRegionIds.Contains(kv.Key)) continue;

            foreach (var entity in kv.Value)
            {
                if (entity.IsInit == 1)
                {
                    // 生成初始化建筑的逻辑
                    // 比如可以实例化预制体
                }
            }
        }
    }

    private async UniTask GenDynamicBuildings()
    {
        foreach (var data in GameEntry.Data.PlayerRoleData.restaurantData.buildings)
        {
            Sys_BuildingsEntity entity = GetBuildingEntity(data.buildingId);
            if (entity != null)
            {
                // 动态生成建筑的逻辑
                // 比如可以根据实体创建预制体实例
            }
        }
    }

    private void InitConfig()
    {
        // 初始化建筑区域数据
        foreach (var kv in GameEntry.DataTable.Sys_BuildingsDBModel.IdByDic)
        {
            Sys_BuildingsEntity entity = kv.Value;
            if (Regions.ContainsKey(entity.RegionId))
            {
                Regions[entity.RegionId].Add(entity);
            }
            else
            {
                Regions[entity.RegionId] = new List<Sys_BuildingsEntity> { entity };
            }
        }

        // 更新解锁的区域 ID
        foreach (var data in GameEntry.Data.PlayerRoleData.restaurantData.buildings)
        {
            int regionId = GetBuildingRegion(data.buildingId);
            UnlockRegionIds.Add(regionId);  // HashSet 会自动去重
        }
    }

    // 获取建筑物的区域 ID
    public int GetBuildingRegion(int buildingId)
    {
        return GetBuildingEntity(buildingId)?.RegionId ?? 1;  // 如果没有找到，返回默认区域 ID
    }

    // 获取建筑物实体
    public Sys_BuildingsEntity GetBuildingEntity(int buildingId)
    {
        GameEntry.DataTable.Sys_BuildingsDBModel.IdByDic.TryGetValue(buildingId, out Sys_BuildingsEntity entity);
        return entity;
    }

    // 是否解锁建筑
    public bool IsUnlockBuilding(int buildingId)
    {
        var buildingIds = new HashSet<int>(GameEntry.Data.PlayerRoleData.restaurantData.buildings.Select(b => b.buildingId));
        return buildingIds.Contains(buildingId);
    }

    public bool UnlockBuilding(int buildingId)
    {
        var buildingIds = new HashSet<int>(GameEntry.Data.PlayerRoleData.restaurantData.buildings.Select(b => b.buildingId));
        Sys_BuildingsEntity entity = GetBuildingEntity(buildingId);
        if (!buildingIds.Contains(buildingId))
        {
            BuildingData data = new BuildingData();
            data.buildingId = buildingId;
            GameEntry.Data.PlayerRoleData.restaurantData.buildings.Add(data);
            if (!UnlockRegionIds.Contains(entity.RegionId))
            {
                UnlockRegionIds.Add(entity.RegionId);
                GenRegionMap();//生成新地图区域
            }
            return true;
        }
        return false;
    }
}
