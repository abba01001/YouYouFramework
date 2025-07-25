using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using YouYou;

public class Farmer : WorkerBase
{
    private Vector3 initialFoodCollectPos;
    public Transform foodCollectPos;
    private Vector3 standPos = new Vector3(2.43f,0.65f,-36.31f);
    public NavMeshAgent agent;
    private Transform targetChickenPos;
    private bool canCheck = true;
    private bool reachedShelf;
    public Animator anim;
    private bool removedAnyFood;

    public PlayerManager _PlayerManager;
    private System.Random _random = new System.Random();

    
    public override void Init(WorkerData data)
    {
        base.Init(data);
        initialFoodCollectPos = foodCollectPos.transform.localPosition;
        agent.updateRotation = true;
        FindChicken();
    }

    public override void Tick()
    {
        if (!IsLiving) return;
        
        if (ReachedDestinationOrGaveUp() && canCheck)
        {
            if (_PlayerManager.collectedFood.Count == _PlayerManager.maxFoodPlayerCarry)
            {
                Goto(targetChickenPos.position);
                canCheck = false;
            }
        }

        if (reachedShelf)
        {
            reachedShelf = false;
            GameEntry.Time.CreateTimer(this, 3, FindChicken);
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            anim.SetBool("Run", false);
        else
            anim.SetBool("Run", true);
    }

    private GameObject[] chickens;

    private void FindChicken()
    {
        foreach (BuildingBase building in BuildingSystem.Instance.GetProduceBuilding())
        {
            if (building.Entity.Produce == "Egg")
            {
                FoodPlaceManager chicken = building.GetComponent<FoodPlaceManager>();
                int j = chicken.collectFoodCapacity / 2;
                if (chicken.collectedFoods.Count < j)
                {
                    targetChickenPos = chicken.HelperPos;
                    FindFoodSpawner(chicken.foodType);
                    return;
                }
            }
        }
        GameEntry.Time.CreateTimer(this, 2, FindChicken);
    }
    private void FindFoodSpawner(FoodType foodType)
    {
        var foodName = foodType.ToString();
        List<FoodSpawner> availableFoodSpawners = new List<FoodSpawner>();
        foreach (BuildingBase shelf in BuildingSystem.Instance.GetShelfBuilding())
        {
            FoodPlaceManager _FoodPlaceManager = shelf.GetComponent<FoodPlaceManager>();
            if (_FoodPlaceManager.foodType != foodType) continue;
            foreach (FoodSpawner foodSpawner in _FoodPlaceManager.foodSpawners)
            {
                availableFoodSpawners.Add(foodSpawner);
            }
        }
        GameUtil.Shuffle(availableFoodSpawners);
        foreach (FoodSpawner foodSpawner in availableFoodSpawners)
        {
            if (foodSpawner.food.foodName == foodName)
            {
                Goto(foodSpawner.transform.position);
                return;
            }
        }
    }

    private void Goto(Vector3 target)
    {
        agent.SetDestination(target);
        canCheck = true;
    }


    private bool ReachedDestinationOrGaveUp()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    return true;
            }
        }

        return false;
    }

    //农民把货物放到鸡场去
    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(Tags.ChickenAnimal))
        {
            FoodPlaceManager shelf = other.GetComponent<FoodPlaceManager>();
            if (shelf.collectedFoods.Count < shelf.collectFoodCapacity)
            {
                int collectedFoodCount = _PlayerManager.collectedFood.Count - 1;

                if (collectedFoodCount >= 0)
                {
                    var foodName = shelf.foodType.ToString();
                    for (int i = _PlayerManager.collectedFood.Count - 1; i >= 0; i--)
                    {
                        if (_PlayerManager.collectedFood[i].foodName == foodName)
                        {
                            removedAnyFood = true;
                            _PlayerManager.collectedFood[i].PlaceFood(shelf.GetIdleFoodTransform());
                            FindObjectOfType<AudioManager>().Play("PlaceFood");

                            shelf.collectedFoods.Add(_PlayerManager.collectedFood[i]);
                            _PlayerManager.collectedFood[i].transform.parent = shelf.transform;

                            _PlayerManager.collectedFood.Remove(_PlayerManager.collectedFood[i]);
                            break;
                        }
                    }

                    if (removedAnyFood)
                    {
                        foodCollectPos.localPosition = initialFoodCollectPos;

                        foreach (Food food in _PlayerManager.collectedFood)
                        {
                            food.transform.localPosition = foodCollectPos.localPosition;
                            foodCollectPos.localPosition = new Vector3(foodCollectPos.transform.localPosition.x, foodCollectPos.transform.localPosition.y + 1, foodCollectPos.transform.localPosition.z);
                        }

                        removedAnyFood = false;
                    }
                }
            }
            else
            {
                Goto(standPos);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.ChickenAnimal))
            reachedShelf = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Tags.ChickenAnimal))
            reachedShelf = false;
    }
}
