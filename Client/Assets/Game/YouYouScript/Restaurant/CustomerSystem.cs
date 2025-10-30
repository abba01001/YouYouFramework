using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public class CustomerSystem
{
    private static CustomerSystem _instance;
    public static CustomerSystem Instance => _instance ??= new CustomerSystem();
    public List<CustomerData> TempCustomerDatas = new List<CustomerData>();
    private List<Customer> TempCustomers = new();
    private readonly Dictionary<int, Customer> idToCustomer = new();

    // 初始化
    public async UniTask Init()
    {

    }

    // 检查是否需要生成新的顾客
    private void AddWorkerIfNeeded()
    {
        if (TempCustomers.Count < 10)//GameEntry.Data.PlayerRoleData.restaurantData.maxCustomerCount)
        {
            // 如果顾客少于上限，增加一个顾客数据
            CustomerData data = new CustomerData();
            data.customerId = GameUtil.RandomRange(1, 9999999);
            data.hatColorIndex = GameUtil.RandomRange(0, GameEntry.Instance.customerColors.Length);
            data.meshColorIndex = GameUtil.RandomRange(0, GameEntry.Instance.customerHats.Length);
            data.mood = CustomerMood.Neutral;
            RandomEmotion(data);
            RandomFood(data);
            TempCustomerDatas.Add(data);
            // 创建顾客对象并加入列表
            SpawnCustomer(data);
        }
    }

    public List<string> FoodConfig = new List<string>()
    {
        "Tomato","Egg"//,"Wheat","Milk","Beans","Bread","Flour"
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
    private void SpawnCustomer(CustomerData data)
    {
        if (idToCustomer.ContainsKey(data.customerId)) return; // 防止重复生成
        
        PoolObj obj = GameEntry.Pool.GameObjectPool.SpawnSynchronous(
            $"Assets/Game/Download/Prefab/Regions/Customer.prefab", BuildingSystem.Instance.Root);
        Customer customer = obj.GetComponent<Customer>();
        customer.Init(data);
        
        int index = GameUtil.RandomRange(0, bornPos.Count);
        obj.gameObject.transform.position = bornPos[index];
        
        TempCustomers.Add(customer);
        idToCustomer[data.customerId] = customer;
    }

    private void HandleFirstCustomer()
    {
        if (GameEntry.Data.PlayerRoleData.restaurantData.maxCustomerCount != 0) return;
        if (BuildingSystem.Instance.IsTest)
        {
            GameEntry.Data.PlayerRoleData.restaurantData.maxCustomerCount++;
            GameEntry.Event.Dispatch(Constants.EventName.TriggerGuideEvent,Constants.EventName.FirstCustomer);
        }
    }

    private float spawnCooldown = 2f; // 每隔2秒生成一个顾客
    private float spawnTimer = 0f;
    // 更新所有顾客
    public void Update()
    {
        spawnTimer += Time.deltaTime;
        HandleFirstCustomer();
        
        for (int i = TempCustomers.Count - 1; i >= 0; i--)
        {
            var customer = TempCustomers[i];
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

    public bool CheckPointIsOccupied(CustomerPoints points)
    {
        foreach (var customer in TempCustomers)
        {
            if (customer._CustomerPoints == points) return true;
        }
        return false;
    }
    
    public void RemoveCustomer(Customer customer)
    {
        if (!TempCustomers.Contains(customer)) return;

        TempCustomers.Remove(customer);
        idToCustomer.Remove(customer.CustomerData.customerId);
        TempCustomerDatas.Remove(customer.CustomerData);
        GameObject.Destroy(customer.gameObject);
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
