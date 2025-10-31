using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using YouYou;


public class BuildingBase : MonoBehaviour
{
    private TriggerDetector triggerDetector;
    public Sys_BuildingsEntity Entity;
    public List<FoodSpawner> foodSpawners =  new List<FoodSpawner>();
    public List<CustomerPoint> customerPoints =  new List<CustomerPoint>();
    public List<InputSlots> shelfPos =  new List<InputSlots>();
    public List<Food> collectedFood =   new List<Food>();
    public void Init(Sys_BuildingsEntity entity)
    {
        Entity =  entity;
        if (triggerDetector == null)
        {
            triggerDetector = gameObject.AddComponent<TriggerDetector>();
        }
        else
        {
            triggerDetector.UnsubscribeAll();
        }
        
        foodSpawners.Clear();
        foreach (var f in transform.GetComponentsInChildren<FoodSpawner>(true))
        {
            foodSpawners.Add(f);
        }
        customerPoints.Clear();
        foreach (var f in transform.GetComponentsInChildren<CustomerPoint>(true))
        {
            customerPoints.Add(f);
        }
        shelfPos.Clear();
        foreach (var f in transform.GetComponentsInChildren<InputSlots>(true))
        {
            shelfPos.Add(f);
        }
        // 绑定触发器事件
        triggerDetector.OnEnter += HandleOnEnter;
        triggerDetector.OnStay += HandleOnStay;
        triggerDetector.OnExit += HandleOnExit;
    }
    
    private void HandleOnEnter(Collider other)
    {

    }
    
    private void HandleOnStay(Collider other)
    {
        if (Entity.BuildingType == "Produce" && other.gameObject.CompareTag("Player"))
        {
            foreach (var foodSpawner in foodSpawners)
            {
                foodSpawner.JumpFoodToPlayer(other);
            }
        }
        if (Entity.BuildingType == "Consume" && other.gameObject.CompareTag("Customer"))
        {
            Customer customer = other.GetComponent<Customer>();
            if (CustomerSystem.Instance.CheckIsFullCollect(customer.CharacterData, Entity.Consume))
            {
                customer.SetFullCollectFlag();
                //释放顾客位置
                return;
            }

            if (collectedFood.Count > 0)
            {
                Food food = collectedFood[collectedFood.Count - 1];
                ClearFoodToBuilding(food);
                customer.CollectFood(food);
            }
        }
    }
    
    private void HandleOnExit(Collider other)
    {

    }
    

    public void PlayUnlockAnim()
    {
        transform.DOPunchScale(new Vector3(0.1f, 1, 0.1f), 0.5f, 7);
    }

    private void OnTriggerEnter(Collider other)
    {

    }
    
    
    public Transform GetIdleFoodTransform()
    {
        return shelfPos[collectedFood.Count].transform;
    }
        
    public CustomerPoint GetIdlePoint()
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
    
    public bool CanPlaceFood()
    {
        return collectedFood.Count < shelfPos.Count;
    }

    public void AddFoodToBuilding(Food food)
    {
        food.PlaceFood(GetIdleFoodTransform());
        food.transform.parent = transform;
        food.goToCustomer = true;
        collectedFood.Add(food);
    }
    
    public void ClearFoodToBuilding(Food food)
    {
        collectedFood.Remove(food);
    }
}
