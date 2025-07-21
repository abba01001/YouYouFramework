using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public class CustomerSystem
{
    private List<Customer> customers = new List<Customer>();
    private static CustomerSystem _instance;

    private CustomerSystem()
    {
    }

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
                GenerateCustomerObj(customerData);
            }
        }
    }

    // 检查是否需要生成新的顾客
    private void CheckSpawnCustomer()
    {
        if (customers.Count < defaultMaxCustomers)
        {
            // 如果顾客少于上限，增加一个顾客数据
            CustomerData data = new CustomerData();
            data.customerId = GameUtil.RandomRange(1, 9999999);
            data.hatColorIndex = GameUtil.RandomRange(0, GameEntry.Instance.customerColors.Length);
            data.meshColorIndex = GameUtil.RandomRange(0, GameEntry.Instance.customerHats.Length);
            data.mood = CustomerMood.Neutral;
            RandomEmotion(data);
            RandomFood(data);
            GameEntry.Data.PlayerRoleData.restaurantData.customers.Add(data);
            // 创建顾客对象并加入列表
            GenerateCustomerObj(data);
        }
    }

    public List<string> FoodConfig = new List<string>()
    {
        "Tomato","Egg"
    };

    public List<(string,int)> GetRandomFoodCombination()
    {
        List<(string,int)> selectedFoods = new List<(string,int)>();
        int count = FoodConfig.Count;

        int randomSelectionCount = GameUtil.RandomRange(1, count + 1);
        List<int> selectedIndices = new List<int>();
        while (selectedIndices.Count < randomSelectionCount)
        {
            int randomIndex = Random.Range(0, count);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
            }
        }

        int max = GameUtil.RandomRange(1,13);
        foreach (int index in selectedIndices)
        {
            int value = 0;
            if (max > 0) value = 1;//GameUtil.RandomRange(1, max);
            selectedFoods.Add((FoodConfig[index],value));
            max -= value;
        }
        selectedFoods.RemoveAll(item => item.Item2 == 0);
        return selectedFoods;
    }
    
    private void RandomFood(CustomerData customerData)
    {
        List<(string,int)> selectedFoodList = GetRandomFoodCombination(); // 获取随机食物组合

        foreach ((string _name, int count) in selectedFoodList)
        {
            FoodData foodData = new FoodData
            {
                name = _name,
                needCount = count
            };
            customerData.foodList.Add(foodData);
        }
        PrintFoordData(customerData);
    }

    public void PrintFoordData(CustomerData customerData)
    {
        return;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("收集食物列表:");
        foreach (FoodData data in customerData.foodList)
        {
            sb.AppendLine($"Food Name: {data.name}, Need Count: {data.needCount}");
        }
        GameUtil.LogError(sb.ToString());
    }

    
    public void RandomEmotion(CustomerData customerData)
    {
        customerData.happyTime = 5;//Random.Range(10, 20);
        customerData.neutralTime = 10;//Random.Range(20, 30);
        customerData.annoyedTime = 12;//Random.Range(30, 40);
        customerData.anxiousTime = 13;//Random.Range(40, 45);
        customerData.angryTime = 15; //Random.Range(45,50);
    }

    public List<Vector3> bornPos = new List<Vector3>()
    {
        new Vector3(16f, 0f, 48f),
        new Vector3(-61f, 0f, -1f),
        new Vector3(-30f,0f,61f),
        new Vector3(-60f,0f,31f)
    };
    private void GenerateCustomerObj(CustomerData data)
    {
        PoolObj obj = GameEntry.Pool.GameObjectPool.SpawnSynchronous(
            $"Assets/Game/Download/Prefab/Regions/Customer.prefab", BuildingSystem.Instance.Root);
        Customer customer = obj.GetComponent<Customer>();
        customer.Init(data);
        customers.Add(customer);
        
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
            GameEntry.Data.PlayerRoleData.restaurantData.customers.Remove(targetData);
            targetCustomer.IsActive = false;
        }
    }

    private float spawnCooldown = 2f; // 每隔2秒生成一个顾客
    private float spawnTimer = 0f;
    // 更新所有顾客
    public void Update()
    {
        spawnTimer += Time.deltaTime;
    
        if (spawnTimer >= spawnCooldown)
        {
            spawnTimer = 0f;
            CheckDeleteCustomer();
            CheckSpawnCustomer();
        }
        
        // 继续更新活跃的顾客
        foreach (var customer in customers)
        {
            if(customer.IsActive) customer.OnUpdate();
        }
    }

    private void CheckDeleteCustomer()
    {
        // 从后往前遍历，删除不活跃的顾客
        for (int i = customers.Count - 1; i >= 0; i--)
        {
            var customer = customers[i];
            if (!customer.IsActive)
            {
                customers.RemoveAt(i);  // 删除不活跃的顾客
                GameObject.Destroy(customer.gameObject);  // 销毁对象
            }
        }
    }

    public bool IsStoreBusy()
    {
        return false;
    }
    
    public bool CheckIsFullCollect(CustomerData data,string foodName = "")
    {
        if (foodName != "")
        {
            foreach (var foodData in data.foodList)
            {
                if (foodData.name == foodName)
                {
                    return foodData.needCount == foodData.hasCount;
                }
            }
            return false;
        }
        foreach (var foodData in data.foodList)
        {
            if (foodData.needCount != foodData.hasCount) return false;
        }
        return true;
    }
}
