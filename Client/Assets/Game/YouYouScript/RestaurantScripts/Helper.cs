using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using YouYou;

public class Helper : WorkerBase
{
    public Vector3 foodCollectPos = new Vector3(0,2.23f,0.93f);
    private Vector3 initialFoodCollectPos;
    private bool removedAnyFood;
    private bool canCheck = true;
    public Animator anim;
    private bool checkReachedShelf;
    public NavMeshAgent agent;
    private Transform targetShelfPos;
    public ParticleSystem upgradeParticle;
    public PlayerManager _PlayerManager;
    private Transform trashBin;
    private GameObject[] shelfs;

    public override void Init(WorkerData data)
    {
        base.Init(data);
        // trashBin = GameObject.FindGameObjectWithTag("TrashBin").transform;
        _PlayerManager.maxFoodPlayerCarry = WorkerData.maxFoodCarry;
        GetComponent<NavMeshAgent>().speed = WorkerData.speed;
        initialFoodCollectPos = foodCollectPos;
        agent.updateRotation = true;
        FindShelf();
    }

    public override void Tick()
    {
        if (!IsLiving) return;
        
        if (ReachedDestinationOrGaveUp() && canCheck)
        {
            if (_PlayerManager.collectedFood.Count >= _PlayerManager.maxFoodPlayerCarry)
            {
                Goto(targetShelfPos.position);
                checkReachedShelf = true;
                canCheck = false;
            }
        }

        if (ReachedDestinationOrGaveUp() && checkReachedShelf)
        {
            if (_PlayerManager.collectedFood.Count == 0)
            {
                checkReachedShelf = false;
                GameEntry.Time.CreateTimer(this, 0.5f, FindShelf);
            }
            else
            {
                
                //agent.SetDestination(trashBin.position);
            }
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            anim.SetBool("Run", false);
        else
            anim.SetBool("Run", true);
    }

    private void FindShelf()
    {
        foreach (BuildingBase shelf in BuildingSystem.Instance.GetShelfBuilding())
        {
            FoodPlaceManager _FoodPlaceManager = shelf.GetComponent<FoodPlaceManager>();
            if (!WorkerData.collectFood.Contains(_FoodPlaceManager.foodType)) continue;
            int i = _FoodPlaceManager.collectFoodCapacity / 2;
            if (_FoodPlaceManager.collectedFoods.Count < i)
            {
                targetShelfPos = _FoodPlaceManager.HelperPos;
                foreach (FoodSpawner foodSpawner in _FoodPlaceManager.foodSpawners)
                {
                    var foodName = _FoodPlaceManager.foodType.ToString();
                    if (foodSpawner.food.foodName == foodName)
                    {
                        if (foodSpawner.foodObj != null)
                        {
                            _PlayerManager.currentFoodName = foodName;
                            Goto(foodSpawner.transform.position + new Vector3(GameUtil.RandomRange(0f,1f),0,GameUtil.RandomRange(0f,1f)));
                            return;
                        }
                    }
                }
            }
        }
        GameEntry.Time.CreateTimer(this, 2, FindShelf);
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

    //协助者把食物放到架子上
    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Shelf"))
        {
            FoodPlaceManager shelf = other.GetComponent<FoodPlaceManager>();
            
            if (shelf.collectedFoods.Count < shelf.collectFoodCapacity)
            {
                int collectedFoodCount = _PlayerManager.collectedFood.Count - 1;
                
                if (collectedFoodCount >= 0)
                {
                    for (int i = _PlayerManager.collectedFood.Count - 1; i >= 0; i--)
                    {
                        Food food = _PlayerManager.collectedFood[i];
                        var foodName = shelf.foodType.ToString();
                        if (food.foodName == foodName)
                        {
                            removedAnyFood = true;
                            food.PlaceFood(shelf.GetIdleFoodTransform());
                            FindObjectOfType<AudioManager>().Play("PlaceFood");

                            shelf.collectedFoods.Add(food);
                            food.transform.parent = shelf.transform;

                            _PlayerManager.collectedFood.Remove(food);
                            break;
                        }
                    }

                    if (removedAnyFood)
                    {
                        foodCollectPos = initialFoodCollectPos;

                        foreach (Food food in _PlayerManager.collectedFood)
                        {
                            food.transform.localPosition = foodCollectPos;
                            foodCollectPos = new Vector3(foodCollectPos.x, foodCollectPos.y + 1, foodCollectPos.z);
                        }
                        removedAnyFood = false;
                    }
                }
            }
        }
    }

    public void IncreaseCapacity(int increaseVal)
    {
        WorkerData.maxFoodCarry += increaseVal;
    }

    public void IncreaseSpeed(int increaseVal)
    {
        GetComponent<NavMeshAgent>().speed += increaseVal;
        WorkerData.speed = GetComponent<NavMeshAgent>().speed;
    }
}
