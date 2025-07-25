using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;
using Random = UnityEngine.Random;

public class CustomerSystem
{
    private static CustomerSystem _instance;
    public static CustomerSystem Instance => _instance ??= new CustomerSystem();
    
    private List<Customer> customers = new();
    private readonly Dictionary<int, Customer> idToCustomer = new();
    private int defaultMaxCustomers = 16;

    public List<FoodType> FoodConfig = new List<FoodType>();
    // 初始化
    public async UniTask Init()
    {
        // 加载已存在的顾客
        var list = GameEntry.Data.PlayerRoleData.restaurantData.customers;
        foreach (var data in list)
        {
            SpawnCustomer(data);
        }
        FoodConfig = Enum.GetValues(typeof(FoodType))
            .Cast<FoodType>()
            .Where(food => food != FoodType.None)  // 排除 None 枚举项
            .ToList();

    }

    // 检查是否需要生成新的顾客
    private void AddWorkerIfNeeded()
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
            SpawnCustomer(data);
        }
    }

    public List<(FoodType, int)> GetRandomFoodCombination()
    {
        List<(FoodType, int)> selectedFoods = new List<(FoodType, int)>();

        List<FoodType> flietrFoods = new List<FoodType>();
        foreach (var building in BuildingSystem.Instance.Buildings)
        {
            if (building.Entity.Produce == "") continue;
            FoodType produce = (FoodType)Enum.Parse(typeof(FoodType), building.Entity.Produce);
            flietrFoods.Add(produce);
        }
        
        int count = flietrFoods.Count;

        int randomSelectionCount = GameUtil.RandomRange(1, count + 1);
        List<int> selectedIndices = new List<int>();
    
        // 随机选择索引
        while (selectedIndices.Count < randomSelectionCount)
        {
            int randomIndex = Random.Range(0, count);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
            }
        }

        int max = GameUtil.RandomRange(1, 13);
    
        // 根据选中的索引，添加食物和数量
        foreach (int index in selectedIndices)
        {
            int value = 0;
            if (max > 0) value = 1; // 可以替换为 GameUtil.RandomRange(1, max);
            selectedFoods.Add((flietrFoods[index], value));
            max -= value;
        }

        // 移除数量为 0 的食物
        selectedFoods.RemoveAll(item => item.Item2 == 0);
        return selectedFoods;
    }
    
    private void RandomFood(CustomerData customerData)
    {
        List<(FoodType, int)> selectedFoodList = GetRandomFoodCombination(); // 获取随机食物组合

        foreach ((FoodType _name, int count) in selectedFoodList)
        {
            FoodData foodData = new FoodData
            {
                foodType = _name,
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
            sb.AppendLine($"Food Name: {data.foodType}, Need Count: {data.needCount}");
        }
        GameUtil.LogError(sb.ToString());
    }

    
    public void RandomEmotion(CustomerData customerData)
    {
        customerData.happyTime = GameUtil.RandomRange(10, 20);
        customerData.neutralTime = GameUtil.RandomRange(20, 30);
        customerData.annoyedTime = GameUtil.RandomRange(30, 40);
        customerData.anxiousTime = GameUtil.RandomRange(40, 45);
        customerData.angryTime = GameUtil.RandomRange(45,50);
    }

    public List<Vector3> bornPos = new List<Vector3>()
    {
        new Vector3(16f, 0f, 48f),
        new Vector3(-61f, 0f, -1f),
        new Vector3(-30f,0f,61f),
        new Vector3(-60f,0f,31f)
    };
    private void SpawnCustomer(CustomerData data)
    {
        if (idToCustomer.ContainsKey(data.customerId)) return; // 防止重复生成
        
        PoolObj obj = GameEntry.Pool.GameObjectPool.SpawnSynchronous(
            $"Assets/Game/Download/Prefab/Regions/Customer.prefab", BuildingSystem.Instance.Root);
        Customer customer = obj.GetComponent<Customer>();
        customer.Init(data);
        
        int index = GameUtil.RandomRange(0, bornPos.Count);
        obj.gameObject.transform.position = bornPos[index];
        
        customers.Add(customer);
        idToCustomer[data.customerId] = customer;
    }


    private float spawnCooldown = 2f; // 每隔2秒生成一个顾客
    private float spawnTimer = 0f;
    // 更新所有顾客
    public void Update()
    {
        spawnTimer += Time.deltaTime;
    
        for (int i = customers.Count - 1; i >= 0; i--)
        {
            var customer = customers[i];
            if (customer.IsLiving)
            {
                customer.Tick();
            }
            else
            {
                RemoveCustomer(customer);
            }
        }
        
        if (spawnTimer >= spawnCooldown)
        {
            spawnTimer = 0f;
            AddWorkerIfNeeded();
        }
    }

    public void RemoveCustomer(Customer customer)
    {
        if (!customers.Contains(customer)) return;

        customers.Remove(customer);
        idToCustomer.Remove(customer.CustomerData.customerId);
        GameEntry.Data.PlayerRoleData.restaurantData.customers.Remove(customer.CustomerData);
        GameObject.Destroy(customer.gameObject);
    }

    public bool IsStoreBusy()
    {
        return false;
    }
    
    public bool CheckIsFullCollect(CustomerData data,FoodType foodType = FoodType.None)
    {
        if (foodType != FoodType.None)
        {
            foreach (var foodData in data.foodList)
            {
                if (foodData.foodType == foodType)
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
