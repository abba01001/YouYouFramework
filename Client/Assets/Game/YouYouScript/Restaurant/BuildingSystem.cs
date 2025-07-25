using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
using UnityEngine.SceneManagement;
using YouYou;

public enum BuildingEnum
{
    CashierDesk,
    Chicken,
    ShelfEgg,
    ShelfTomato,
    PlantSet,
    ShelfSauce,
    KitchenCounter,
    PlantWheat,
    ShelfWheat,
    CowAnimal,
    ShelfFridge,
    PlantCoffee,
    ShelfCoffee,
    BreadMachine,
    FlourMachine,
    ShelfBread,
    ShelfFlour
}

public enum FoodType
{
    None,
    Tomato,
    Egg,
    Wheat,
    Milk,
    Beans,
    Bread,
    Flour,
    Sauce
}

public static class Tags
{
    public const string ChickenAnimal = "ChickenAnimal";
    public const string Shelf = "Shelf";
    // 添加其他标签
}

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
    private List<BuyBuildingPoint> _buyBuildingPoints = new List<BuyBuildingPoint>();
    public IReadOnlyList<BuildingBase> Buildings => _buildings;
    private Dictionary<int, GameObject> RegionMap = new Dictionary<int, GameObject>();
    private HashSet<int> UnlockRegionIds = new HashSet<int> { 1 };  // 初始化时加入默认的区域 1
    private HashSet<int> _buildingIdsCache = new HashSet<int>();
    private HashSet<int> _buyPointIdsCache = new HashSet<int>();
    public PlayerController PlayerController;

    public bool InitFinish { get; set; }
    public Transform Root { get; private set; }
    public async UniTask Init()
    {
        Scene targetScene = SceneManager.GetSceneByName("Game");
        if (targetScene.IsValid())
        {
            Root = new GameObject("Root").transform;
            SceneManager.MoveGameObjectToScene(Root.gameObject, targetScene);
        }
        
        InitConfig();
        await GenRegionMap();  // 生成地图
        await GenInitBuildings();  // 生成初始建筑
        await GenHasBuildings();  // 生成已经购买的建筑
        await GenBuyBuildingPoint(); //生成购买建筑点
        await GenPlayer(); // 生成玩家
        await CustomerSystem.Instance.Init(); // 生成顾客
        await WorkerSystem.Instance.Init(); // 生成协助者
        GameEntry.Event.Dispatch(Constants.EventName.UpdateBuildingsObj,null);
        InitFinish = true;
    }

    private async UniTask GenPlayer()
    {
        // 玩家生成逻辑
        PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/Role/Player.prefab",Root);
        obj.transform.position = new Vector3(5, 0, 21);
        obj.gameObject.MSetActive(true);
        // Scene targetScene = SceneManager.GetSceneByName("Main");
        // if (targetScene.IsValid())
        // {
        //     SceneManager.MoveGameObjectToScene(obj.gameObject, targetScene);
        // }
        GameEntry.Event.Dispatch(Constants.EventName.UpdateFoodPlayerCarry,obj.GetComponent<PlayerManager>().maxFoodPlayerCarry);
        PlayerController = obj.GetComponent<PlayerController>();
            
                        
        GameEntry.UI.OpenUIForm<FormMain>();
        GameEntry.Audio.PlayBGM("Home");
    }

    public List<int> noRegionsId = new List<int>() { 3 };
    private async UniTask GenRegionMap()
    {
        foreach (var id in UnlockRegionIds)
        {
            if (RegionMap.ContainsKey(id)) continue;
            if (noRegionsId.Contains(id)) continue;
            PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/Regions/Area{id}.prefab",Root);
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
                if (entity.Cost == 0)
                {
                    if (GetBuildingData(entity.BuildingId) == null)
                    {
                        BuildingData data = new BuildingData { buildingId = entity.BuildingId };
                        GameEntry.Data.PlayerRoleData.restaurantData.buildings.Add(data);
                    }
                    await CreateBuilding(entity);
                }
            }
        }
    }

    private async UniTask GenBuyBuildingPoint()
    {
        foreach (var kv in Regions)
        {
            if (!UnlockRegionIds.Contains(kv.Key)) continue;
            foreach (var parent in kv.Value)
            {
                List<int> dependenCies = GameUtil.ParseNumbers(parent.Dependencies);
                if (!IsUnlockBuilding(parent.BuildingId))
                {
                    if (dependenCies.Count > 0 || parent.isVisible == 1)
                    {
                        await CreateBuyPoint(parent);
                    }
                }
                foreach (var id in dependenCies)
                {
                    Sys_BuildingsEntity child = GetBuildingEntity(id);
                    if (child != null)
                    {
                        if (child.isVisible == 1)
                        {
                            if (!IsUnlockBuilding(child.BuildingId))
                            {
                                await CreateBuyPoint(child);
                            }
                        }
                        else
                        {
                            if (IsUnlockBuilding(parent.BuildingId) && !IsUnlockBuilding(child.BuildingId))
                            {
                                await CreateBuyPoint(child);
                            }
                        }
                    }
                }
            }
        }
    }
    
    private async UniTask GenHasBuildings(bool unlock = false)
    {
        foreach (var data in GameEntry.Data.PlayerRoleData.restaurantData.buildings)
        {
            Sys_BuildingsEntity entity = GetBuildingEntity(data.buildingId);
            if (entity != null)
            {
                await CreateBuilding(entity,unlock);
            }
        }
    }

    private async UniTask CreateBuilding(Sys_BuildingsEntity entity,bool isUnlock = false)
    {
        if (CheckHasBuildingObj(entity.BuildingId)) return;
        if (entity == null) return;

        PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/Regions/{entity.BuildingName}.prefab", RegionMap[entity.RegionId].transform);
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
            BuildingSystem.Instance.PlayerController.SidePos();
        }
    }

    private async UniTask CreateBuyPoint(Sys_BuildingsEntity entity)
    {
        if (CheckHasBuyPoint(entity.BuildingId)) return;
        if (entity == null) return;
        
        PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/Regions/BuyBuildingPoint.prefab", RegionMap[entity.RegionId].transform);
        BuyBuildingPoint building = obj.GetComponent<BuyBuildingPoint>();
        building.Init(entity);
        obj.gameObject.MSetActive(true);
        obj.transform.position = GameUtil.ParseCoordinates(entity.Position);
        _buyBuildingPoints.Add(building);
        _buyPointIdsCache.Add(entity.BuildingId); // 缓存建筑ID，避免重复创建
    }
    
    private bool CheckHasBuildingObj(int buildingId)
    {
        return _buildingIdsCache.Contains(buildingId);
    }

    private bool CheckHasBuyPoint(int buildingId)
    {
        return _buyPointIdsCache.Contains(buildingId);
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
        
        //检测是否解锁新区域
        CheckUnlockNewRegion();
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

    public BuildingData GetBuildingData(int buildingId)
    {
        foreach (var data in GameEntry.Data.PlayerRoleData.restaurantData.buildings)
        {
            if (data.buildingId == buildingId)
            {
                return data;
            }
        }

        return null;
    }
    
    public List<BuildingBase> GetBuildingByType(BuildingEnum name)
    {
        List<BuildingBase> list = new List<BuildingBase>();
        var n = name.ToString();
        foreach (var building in _buildings)
        {
            if (building.Entity.BuildingName == n)
            {
                list.Add(building);
            }
        }
        return list;
    }

    //获取收银台
    public BillingDesk GetBillingDesk()
    {
        List<BuildingBase> buildings = GetBuildingByType(BuildingEnum.CashierDesk);
        int id = GameUtil.RandomRange(0, buildings.Count);
        
        return buildings[id].GetComponentInChildren<BillingDesk>(true);
    }
    
    // 是否解锁建筑
    public bool IsUnlockBuilding(int buildingId)
    {
        if (GetBuildingEntity(buildingId).Cost == 0) return true;
        var buildingIds = new HashSet<int>(GameEntry.Data.PlayerRoleData.restaurantData.buildings.Select(b => b.buildingId));
        return buildingIds.Contains(buildingId);
    }

    private float countAnimSpeed = 0.1f;
    private TimeAction spendAction;
    public void SpendCoinToUnlock(int buildingId)
    {
        if (IsUnlockBuilding(buildingId)) return;
        Sys_BuildingsEntity entity = GetBuildingEntity(buildingId);
        if (entity == null) return;
        
        if (!GameEntry.Data.PlayerRoleData.restaurantData.readyUnlocks.ContainsKey(buildingId))
        {
            GameEntry.Data.PlayerRoleData.restaurantData.readyUnlocks.Add(entity.BuildingId, 0);
        }

        long hasMoney = GameEntry.Data.Coin;
        if (hasMoney > 100)
            countAnimSpeed = 0.05f;
        else if (hasMoney > 500)
            countAnimSpeed = 0.01f;
        
        spendAction?.Stop();
        spendAction = GameEntry.Time.CreateTimerLoop(this, countAnimSpeed, -1, (_) =>
        {
            if (GameEntry.Data.Coin == 0)
            {
                StopSpend();
                GameUtil.ShowTip("金币不足~");
                return;
            }
            GameEntry.Data.PlayerRoleData.restaurantData.readyUnlocks.TryGetValue(entity.BuildingId, out int currentCount);
            GameEntry.Data.PlayerRoleData.restaurantData.readyUnlocks[entity.BuildingId] = currentCount + 1;
            GameEntry.Data.LessMoney(1);
            UpdateBuildingSpendEvent e = MainEntry.ClassObjectPool.Dequeue<UpdateBuildingSpendEvent>();
            e.Init(entity.BuildingId,1,currentCount == entity.Cost);
            GameEntry.Event.Dispatch(Constants.EventName.UpdateBuildingSpend,e);
            if (currentCount == entity.Cost)
            {
                UnlockBuilding(entity.BuildingId);
                StopSpend();
                return;
            }
        });
    }

    public void RemoveBuyBuildingPoint(BuyBuildingPoint point)
    {
        point.gameObject.MSetActive(false);
    }
    
    public void StopSpend()
    {
        spendAction?.Stop();
    }

    public async UniTask UnlockBuilding(int buildingId)
    {
        var buildingIds = new HashSet<int>(GameEntry.Data.PlayerRoleData.restaurantData.buildings.Select(b => b.buildingId));
        Sys_BuildingsEntity entity = GetBuildingEntity(buildingId);
        if (entity != null && !buildingIds.Contains(buildingId))
        {
            BuildingData data = new BuildingData { buildingId = buildingId };
            GameEntry.Data.PlayerRoleData.restaurantData.buildings.Add(data);
            await GenHasBuildings(true);  // 生成已经购买的建筑

            // 更新解锁的区域
            if (CheckUnlockNewRegion())
            {
                await GenRegionMap();       // 生成区域地图
                await GenInitBuildings();   // 生成建筑
            }
            await GenBuyBuildingPoint(); //生成购买建筑点
            GameEntry.Event.Dispatch(Constants.EventName.UpdateBuildingsObj,null);
        }
    }

    private bool CheckUnlockNewRegion()
    {
        List<int> hasUnlockRegions = new List<int>() { 1 };
        bool hasNewRegion = false;

        var builtIds = GameEntry.Data.PlayerRoleData.restaurantData.buildings
            .Select(b => b.buildingId)
            .ToHashSet(); // 提高查找效率

        foreach (var kv in Regions)
        {
            int regionId = kv.Key;
            List<Sys_BuildingsEntity> regionBuildings = kv.Value
                .Where(entity => !(entity.Cost == 0))
                .ToList();
            
            List<int> missingIds = new List<int>();
            foreach (var entity in regionBuildings)
            {
                if (!builtIds.Contains(entity.BuildingId))
                {
                    missingIds.Add(entity.BuildingId);
                }
            }

            if (missingIds.Count == 0)
            {
                hasUnlockRegions.Add(regionId + 1);
            }
            else
            {
                // GameUtil.LogError($"区域 {regionId + 1} 未解锁，缺少建筑 ID：{string.Join(", ", missingIds)}");
            }
        }

        foreach (var regionId in hasUnlockRegions)
        {
            if (!UnlockRegionIds.Contains(regionId))
            {
                UnlockRegionIds.Add(regionId);
                hasNewRegion = true;
            }
        }

        return hasNewRegion;
    }


    
    //获取货架建筑
    public GameObject GetShelfBuilding(FoodType foodType)
    {
        var food = foodType.ToString();
        foreach (var build in _buildings)
        {
            if (build.Entity.BuildingType == "Shelf" && build.Entity.Consume == food)
            {
                return build.gameObject;
            }
        }
        return null;
    }
    
    public List<BuildingBase> GetShelfBuilding()
    {
        List<BuildingBase> list = new List<BuildingBase>();
        foreach (var build in _buildings)
        {
            if (build.Entity.BuildingType == "Shelf")
            {
                list.Add(build);
            }
        }
        return list;
    }
    
    public List<BuildingBase> GetProduceBuilding()
    {
        List<BuildingBase> list = new List<BuildingBase>();
        foreach (var build in _buildings)
        {
            if (build.Entity.BuildingType == "Produce")
            {
                list.Add(build);
            }
        }
        return list;
    }

    public bool HasPromotionActivity()
    {
        return false;
    }
}
