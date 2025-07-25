using System;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class FoodPlaceManager : MonoBehaviour
{
    public Transform HelperPos;
    public int collectFoodCapacity;

    public FoodType foodType;
    public List<CustomerPoints> customerPoints;

    [HideInInspector] public List<Food> collectedFoods;
    public List<Transform> shelfPos;
    public List<FoodSpawner> foodSpawners =  new List<FoodSpawner>();

    public Transform GetIdleFoodTransform()
    {
        return shelfPos[collectedFoods.Count];
    }

    private void OnEnable()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.UpdateBuildingsObj, OnUpdateBuildingsObj);
    }

    private void OnUpdateBuildingsObj(object userdata)
    {
        foodSpawners.Clear();
        var foodType = this.foodType.ToString();
        foreach (BuildingBase building in BuildingSystem.Instance.GetProduceBuilding())
        {
            if (building.Entity.Produce == foodType)
            {
                foodSpawners.Add(building.GetComponentInChildren<FoodSpawner>(true));
            }
        }
    }

    private void OnDisable()
    {
        GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateBuildingsObj, null);
    }

    private readonly object lockObject = new object();
    public CustomerPoints GetIdlePoint()
    {
        lock (lockObject)  // 锁定操作，保证同一时刻只有一个线程能进入
        {
            foreach (var point in customerPoints)
            {
                if (!point.fill)
                {
                    point.fill = true;  // 标记为已填充
                    return point;  // 返回找到的空闲点
                }
            }
        }
        return null;  // 如果没有找到空闲点，返回 null
    }
}