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

    private void Awake()
    {
        customerPoints.Clear();
        foreach (var VARIABLE in transform.GetComponentsInChildren<CustomerPoints>(true))
        {
            customerPoints.Add(VARIABLE);   
        }
    }

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

    public CustomerPoints GetIdlePoint()
    {
        foreach (var point in customerPoints)
        {
            if (!point.fill)
            {
                return point;
            }
        }
        return null; 
    }
}