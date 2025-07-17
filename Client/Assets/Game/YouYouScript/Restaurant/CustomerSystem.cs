using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public class CustomerSystem
{
    private List<Customer> customers = new List<Customer>();
    private static CustomerSystem _instance;

    private CustomerSystem() { }

    public static CustomerSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CustomerSystem();
            }
            return _instance;
        }
    }

    private int defaultMaxCustomers = 4;

    // 初始化
    public async UniTask Init()
    {
        // 加载已存在的顾客
        if (GameEntry.Data.PlayerRoleData.restaurantData.customers.Count > 0)
        {
            foreach (var customerData in GameEntry.Data.PlayerRoleData.restaurantData.customers)
            {
                // 这里假设你有顾客预制体生成的逻辑
                float spawnTime = GameUtil.RandomRange(0f, 4f);
                GameEntry.Time.CreateTimer(this, spawnTime, GenerateCustomerObj);
            }
        }

        // 每秒钟检查生成顾客
        GameEntry.Time.CreateTimerLoop(this, 1f, -1, (_) =>
        {
            CheckSpawnCustomer();
        });
    }

    // 检查是否需要生成新的顾客
    private void CheckSpawnCustomer()
    {
        if (customers.Count < defaultMaxCustomers)
        {
            // 如果顾客少于上限，增加一个顾客数据
            CustomerData data = new CustomerData();
            GameEntry.Data.PlayerRoleData.restaurantData.customers.Add(data);

            // 创建顾客对象并加入列表
            float spawnTime = GameUtil.RandomRange(0f, 4f);
            GameEntry.Time.CreateTimer(this, spawnTime, GenerateCustomerObj);
        }
    }

    public List<Vector3> bornPos = new List<Vector3>()
    {
        new Vector3(16, 5, 48),
        new Vector3(-61, 5, -1),
        new Vector3(-30,2,61),
        new Vector3(-60,2,31)
    };
    private void GenerateCustomerObj()
    {
        PoolObj obj = GameEntry.Pool.GameObjectPool.SpawnSynchronous(
            $"Assets/Game/Download/Prefab/Regions/Customer.prefab", BuildingSystem.Instance.Root);
        customers.Add(obj.GetComponent<Customer>());
        int index = GameUtil.RandomRange(0, bornPos.Count);
        obj.gameObject.transform.position = bornPos[index];
    }

    // 移除顾客
    public void RemoveCustomer(Customer targetCustomer)
    {
        var targetData = GameEntry.Data.PlayerRoleData.restaurantData.customers
            .FirstOrDefault(customer => customer.customerId == targetCustomer.CustomerData.customerId);

        if (targetData != null)
        {
            // 确保数据一致性，删除顾客
            GameEntry.Data.PlayerRoleData.restaurantData.customers.Remove(targetData);
            targetCustomer.IsActive = false;
            customers.Remove(targetCustomer);
            GameObject.Destroy(targetCustomer.gameObject);
        }
    }

    // 更新所有顾客
    public void Update()
    {
        for (int i = customers.Count - 1; i >= 0; i--)
        {
            var customer = customers[i];
            // customer.Update();

            // 如果顾客不再活跃，移除顾客
            if (!customer.IsActive) 
            {
                customers.RemoveAt(i);  // 从后往前删除，避免索引错乱
                RemoveCustomer(customer); // 同时从游戏数据中移除
            }
        }
    }
}
