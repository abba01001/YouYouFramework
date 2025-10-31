using System;
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

    private readonly List<CharacterBase> workers = new();
    private readonly Dictionary<string, CharacterBase> idToWorker = new();

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
        var list = GameEntry.Data.PlayerRoleData.restaurantData.workers;
        foreach (var data in list)
        {
            SpawnWorker(data);
        }
    }

    private void SpawnWorker(CharacterData data)
    {
        if (idToWorker.ContainsKey(data.characterId)) return; // 防止重复生成

        var prefabPath = $"Assets/Game/Download/Prefab/Regions/{data.type}.prefab";
        PoolObj obj = GameEntry.Pool.GameObjectPool.SpawnSynchronous(prefabPath, BuildingSystem.Instance.Root);

        CharacterBase characterBase = obj.GetComponent<CharacterBase>();
        characterBase.Init(data);

        data.characterId = Guid.NewGuid().ToString();
        Vector3 pos = GetRandomBornPos();
        obj.transform.position = pos;
        obj.gameObject.MSetActive(true);

        workers.Add(characterBase);
        idToWorker[data.characterId] = characterBase;
    }

    public void RemoveWorker(CharacterBase characterBase)
    {
        if (!workers.Contains(characterBase)) return;

        workers.Remove(characterBase);
        idToWorker.Remove(characterBase.CharacterData.characterId);
        GameEntry.Data.PlayerRoleData.restaurantData.workers.Remove(characterBase.CharacterData);
        GameObject.Destroy(characterBase.gameObject);
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

    public void AddWorkerIfNeeded()
    {
        var restaurantData = GameEntry.Data.PlayerRoleData.restaurantData;

        int helperCount = workers.Count(w => w.CharacterData.type == WorkerType.Helper);
        int cashierCount = workers.Count(w => w.CharacterData.type == WorkerType.Cashier);
        int farmerCount = workers.Count(w => w.CharacterData.type == WorkerType.Farmer);

        if (helperCount < restaurantData.maxHelperCount)
        {
            var data = new CharacterData() { type = WorkerType.Helper, characterId = Guid.NewGuid().ToString() };
            var selectedFoodList = CustomerSystem.Instance.GetRandomFoodCombination();
            foreach (var item in selectedFoodList)
                data.collectFood.Add(item.Item1);
        
            restaurantData.workers.Add(data);
            SpawnWorker(data);
        }

        if (cashierCount < restaurantData.maxCashierCount)
        {
            var data = new CharacterData() { type = WorkerType.Cashier, characterId = Guid.NewGuid().ToString() };
            restaurantData.workers.Add(data);
            SpawnWorker(data);
        }

        if (farmerCount < restaurantData.maxFarmerCount)
        {
            var data = new CharacterData() { type = WorkerType.Farmer, characterId = Guid.NewGuid().ToString() };
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
