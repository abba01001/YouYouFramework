using System;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class FoodPlaceManager : MonoBehaviour
{
    public Transform HelperPos;
    public int collectFoodCapacity;

    public string shelfFoodName;
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
        foreach (BuildingBase building in BuildingSystem.Instance.GetpProduceBuilding())
        {
            if (building.Entity.Produce == shelfFoodName)
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
        lock (lockObject) 
        {
            foreach (var point in customerPoints)
            {
                if (!point.fill)
                {
                    point.fill = true;
                    return point;
                }
            }
        }
        return null; 
    }
}