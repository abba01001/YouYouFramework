using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public enum WorkerType
{
    Farmer,
    Cashier,
    Helper,
    // 你可以继续扩展，例如 Cleaner, Security 等等
}

public class WorkerSystem
{
    private static WorkerSystem _instance;
    public static WorkerSystem Instance => _instance ??= new WorkerSystem();

    private readonly List<WorkerBase> workers = new();
    private readonly Dictionary<int, WorkerBase> idToWorker = new();

    public List<Vector3> bornPos = new()
    {
        new Vector3(16, 5, 48),
        new Vector3(-61, 5, -1),
        new Vector3(-30, 2, 61),
        new Vector3(-60, 2, 31)
    };

    private float spawnCooldown = 2f; // 2秒生成间隔
    private float spawnTimer = 0f;

    public async UniTask Init()
    {
        return;
        var list = GameEntry.Data.PlayerRoleData.restaurantData.workers;
        foreach (var data in list)
        {
            SpawnWorker(data);
        }
    }

    private void SpawnWorker(WorkerData data)
    {
        if (idToWorker.ContainsKey(data.workerId)) return; // 防止重复生成

        var prefabPath = $"Assets/Game/Download/Prefab/Regions/{data.type}.prefab";
        PoolObj obj = GameEntry.Pool.GameObjectPool.SpawnSynchronous(prefabPath, BuildingSystem.Instance.Root);

        WorkerBase workerBase = obj.GetComponent<WorkerBase>();
        workerBase.Init(data);

        data.workerId = data.workerId == 0 ? GameUtil.RandomRange(1, 9999999) : data.workerId;
        Vector3 pos = GetRandomBornPos();
        obj.transform.position = pos;
        obj.gameObject.MSetActive(true);

        workers.Add(workerBase);
        idToWorker[data.workerId] = workerBase;
    }

    public void RemoveWorker(WorkerBase workerBase)
    {
        if (!workers.Contains(workerBase)) return;

        workers.Remove(workerBase);
        idToWorker.Remove(workerBase.WorkerData.workerId);
        GameEntry.Data.PlayerRoleData.restaurantData.workers.Remove(workerBase.WorkerData);
        GameObject.Destroy(workerBase.gameObject);
    }

    public void Update()
    {
        spawnTimer += Time.deltaTime;

        for (int i = workers.Count - 1; i >= 0; i--)
        {
            var worker = workers[i];
            if (worker.IsLiving)
            {
                worker.Tick();
            }
            else
            {
                RemoveWorker(worker);
            }
        }

        if (spawnTimer >= spawnCooldown)
        {
            spawnTimer = 0f;
            AddWorkerIfNeeded();
        }
    }

    public int index = 1;
    public void AddWorkerIfNeeded()
    {
        var restaurantData = GameEntry.Data.PlayerRoleData.restaurantData;

        int helperCount = workers.Count(w => w.WorkerData.type == WorkerType.Helper);
        int cashierCount = workers.Count(w => w.WorkerData.type == WorkerType.Cashier);
        int farmerCount = workers.Count(w => w.WorkerData.type == WorkerType.Farmer);

        // if (helperCount < restaurantData.maxHelperCount)
        // {
        //     var data = new WorkerData() { type = WorkerType.Helper, workerId = 0 };
        //     var selectedFoodList = CustomerSystem.Instance.GetRandomFoodCombination();
        //     foreach (var item in selectedFoodList)
        //         data.collectFood.Add(item.Item1);
        //
        //     restaurantData.workers.Add(data);
        //     SpawnWorker(data);
        // }

        if (workers.Count < index)
        {
            var data = new WorkerData() { type = WorkerType.Helper, workerId = 0 };
            var selectedFoodList = CustomerSystem.Instance.GetRandomFoodCombination();
            foreach (var item in selectedFoodList)
                data.collectFood.Add(item.Item1);

            restaurantData.workers.Add(data);
            SpawnWorker(data);
        }

        if (cashierCount < restaurantData.maxCashierCount)
        {
            var data = new WorkerData() { type = WorkerType.Cashier, workerId = 0 };
            restaurantData.workers.Add(data);
            SpawnWorker(data);
        }

        if (farmerCount < restaurantData.maxFarmerCount)
        {
            var data = new WorkerData() { type = WorkerType.Farmer, workerId = 0 };
            data.collectFood.Add("Tomato");
            restaurantData.workers.Add(data);
            SpawnWorker(data);
        }
    }

    private Vector3 GetRandomBornPos()
    {
        return bornPos[GameUtil.RandomRange(0, bornPos.Count)];
    }
}
