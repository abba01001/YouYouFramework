using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public class WorkerSystem
{
    private List<Worker> workers = new List<Worker>();
    private static WorkerSystem _instance;

    private WorkerSystem() { }

    public static WorkerSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WorkerSystem();
            }
            return _instance;
        }
    }

    // ?????
    public async UniTask Init()
    {
        // ????????????
        if (GameEntry.Data.PlayerRoleData.restaurantData.workers.Count > 0)
        {
            foreach (var workderData in GameEntry.Data.PlayerRoleData.restaurantData.workers)
            {
                // ??????????§Û???????????????
                float spawnTime = GameUtil.RandomRange(0f, 4f);
                GameEntry.Time.CreateTimer(this, spawnTime, () =>
                {
                    GenerateHelperObj(workderData);
                });
            }
        }
    }
    
    public List<Vector3> bornPos = new List<Vector3>()
    {
        new Vector3(16, 5, 48),
        new Vector3(-61, 5, -1),
        new Vector3(-30,2,61),
        new Vector3(-60,2,31)
    };
    private void GenerateHelperObj(WorkerData data)
    {
        PoolObj obj = GameEntry.Pool.GameObjectPool.SpawnSynchronous(
            $"Assets/Game/Download/Prefab/Regions/{data.type}.prefab", BuildingSystem.Instance.Root);
        if (data.type == "Helper")
        {
            Helper helper = obj.GetComponent<Helper>();
            helper.Init(data);
            workers.Add(helper);
        }
        else if (data.type == "Cashier")
        {
            Cashier cashier = obj.GetComponent<Cashier>();
            cashier.Init(data);
            workers.Add(cashier);
        }

        data.workerId = GameUtil.RandomRange(1, 9999999);
        int index = GameUtil.RandomRange(0, bornPos.Count);
        obj.gameObject.transform.position = bornPos[index];
    }

    private void CheckSpawnWorker()
    {
        if (GetHelperObjCount < GameEntry.Data.PlayerRoleData.restaurantData.maxHelperCount)
        {
            // ?????????????????????????????
            WorkerData data = new WorkerData();
            data.type = "Helper";
            List<(string,int)> selectedFoodList = CustomerSystem.Instance.GetRandomFoodCombination(); // ????????????
            foreach (var item in selectedFoodList)
            {
                data.collectFood.Add(item.Item1);
            }
            PrintFoordData(data);
            GameEntry.Data.PlayerRoleData.restaurantData.workers.Add(data);
            GenerateHelperObj(data);
        }
        if (GetCashierObjCount < GameEntry.Data.PlayerRoleData.restaurantData.maxCashierCount)
        {
            // ?????????????????????????????
            WorkerData data = new WorkerData();
            data.type = "Cashier";
            GameEntry.Data.PlayerRoleData.restaurantData.workers.Add(data);
            GenerateHelperObj(data);
        }
    }

    private int GetHelperObjCount => workers.Count(worker => worker.WorkerData.type == "Helper");
    private int GetCashierObjCount => workers.Count(worker => worker.WorkerData.type == "Cashier");
    
    public void PrintFoordData(WorkerData customerData)
    {
        return;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("???????§Ò?:");
        foreach (string str in customerData.collectFood)
        {
            sb.AppendLine($"Food Name: {str}");
        }
        GameUtil.LogError(sb.ToString());
    }

    // ??????
    public void RemoveWorker(Worker targetWorker)
    {
        var targetData = GameEntry.Data.PlayerRoleData.restaurantData.workers
            .FirstOrDefault(customer => customer.workerId == targetWorker.WorkerData.workerId);

        if (targetData != null)
        {
            // ???????????????????
            GameEntry.Data.PlayerRoleData.restaurantData.workers.Remove(targetData);
            workers.Remove(targetWorker);
            GameObject.Destroy(targetWorker.gameObject);
        }
    }

    // ???????§Û??
    public void Update()
    {
        CheckSpawnWorker();
        for (int i = workers.Count - 1; i >= 0; i--)
        {
            var worker = workers[i];
            worker.OnUpdate();
            if (!worker.IsActive) 
            {
                workers.RemoveAt(i);  // ???????????????????????
                RemoveWorker(worker); // ????????????????
            }
        }
    }
}
