using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public class BuildingSystem
{
    private static BuildingSystem _instance;
    private BuildingSystem() { }

    public static BuildingSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BuildingSystem();
            }
            return _instance;
        }
    }

    public void Update()
    {
        
    }

    private Dictionary<int, List<Sys_BuildingsEntity>> Regions = new Dictionary<int, List<Sys_BuildingsEntity>>();
    private List<BuildingBase> _buildings = new List<BuildingBase>();
    public IReadOnlyList<BuildingBase> Buildings => _buildings;
    private Dictionary<int, GameObject> RegionMap = new Dictionary<int, GameObject>();
    private HashSet<int> UnlockRegionIds = new HashSet<int> { 1 };  // 初始化时加入默认的区域 1
    private HashSet<int> _buildingIdsCache = new HashSet<int>();

    public async UniTask Init()
    {
        InitConfig();
        await GenRegionMap();  // 生成地图
        await GenInitBuildings();  // 生成初始建筑
        await GenDynamicBuildings();  // 生成动态建筑
        await CustomerSystem.Instance.Init(); // 生成顾客
        await HelperSystem.Instance.Init(); // 生成协助者
        await GenPlayer(); // 生成玩家
    }

    private async UniTask GenPlayer()
    {
        // 玩家生成逻辑
    }

    private async UniTask GenRegionMap()
    {
        foreach (var id in UnlockRegionIds)
        {
            if (RegionMap.ContainsKey(id)) continue;
            PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/Regions/Area{id}.prefab");
            obj.gameObject.MSetActive(true);
            RegionMap.Add(id, obj.gameObject);
        }
    }

    public void Test()
    {
        foreach (var kv in Buildings)
        {
            kv.PlayUnlockAnim();
            break;
        }
    }

    private async UniTask GenInitBuildings()
    {
        foreach (var kv in Regions)
        {
            if (!UnlockRegionIds.Contains(kv.Key)) continue;

            foreach (var entity in kv.Value)
            {
                if (entity.IsInit == 1)
                {
                    await CreateBuilding(entity);
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
                await CreateBuilding(entity);
            }
        }
    }

    private async UniTask CreateBuilding(Sys_BuildingsEntity entity,bool isUnlock = false)
    {
        if (CheckHasBuildingObj(entity.BuildingId)) return;
        if (entity == null) return;

        PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/Regions/{entity.BuildingType}.prefab", RegionMap[entity.RegionId].transform);
        BuildingBase building = obj.GetComponent<BuildingBase>();
        building.Init(entity);
        obj.gameObject.MSetActive(true);
        obj.transform.position = GameUtil.ParseCoordinates(entity.Position);
        obj.transform.rotation = Quaternion.Euler(GameUtil.ParseCoordinates(entity.Rotation));
        _buildings.Add(building);
        _buildingIdsCache.Add(entity.BuildingId); // 缓存建筑ID，避免重复创建
        if (isUnlock)
        {
            building.PlayUnlockAnim();
        }
    }

    private bool CheckHasBuildingObj(int buildingId)
    {
        return _buildingIdsCache.Contains(buildingId);
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

    public int GetBuildingRegion(int buildingId)
    {
        return GetBuildingEntity(buildingId)?.RegionId ?? 1;  // 如果没有找到，返回默认区域 ID
    }

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

    public async UniTask UnlockBuilding(int buildingId)
    {
        var buildingIds = new HashSet<int>(GameEntry.Data.PlayerRoleData.restaurantData.buildings.Select(b => b.buildingId));
        Sys_BuildingsEntity entity = GetBuildingEntity(buildingId);
        if (entity != null && !buildingIds.Contains(buildingId))
        {
            BuildingData data = new BuildingData { buildingId = buildingId };
            GameEntry.Data.PlayerRoleData.restaurantData.buildings.Add(data);

            // 更新解锁的区域
            if (!UnlockRegionIds.Contains(entity.RegionId))
            {
                UnlockRegionIds.Add(entity.RegionId);
                await GenRegionMap();  // 生成新地图区域
            }
            await CreateBuilding(entity);
        }
    }
}
