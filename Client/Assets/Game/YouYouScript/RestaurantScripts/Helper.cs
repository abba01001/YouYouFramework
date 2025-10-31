using System;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using YouYou;

public class Helper : WorkerBase
{
    public Transform foodCollectPos;
    private Vector3 initialFoodCollectPos;
    private bool removedAnyFood;
    public Animator anim;
    public ParticleSystem upgradeParticle;
    private Transform trashBin;
    private GameObject[] shelfs;
    
    public override void Init(WorkerData data)
    {
        base.Init(data);
        // trashBin = GameObject.FindGameObjectWithTag("TrashBin").transform;
        GetComponent<NavMeshAgent>().speed = WorkerData.speed;
        initialFoodCollectPos = foodCollectPos.transform.localPosition;
        agent.updateRotation = true;
    }

    public override void StartWork()
    {
        base.StartWork();

    }

    public override void Tick()
    {
        if (!IsLiving) return;
        if (isStateChanged)
        {
            isStateChanged = false;
            switch (CurrentState)
            {
                case WorkerState.Idle:
                    HandleIdleRun();
                    break;
                case WorkerState.GoToProduceBuilding:
                    HandleFindProduce();
                    break;
                case WorkerState.GoToShelfBuilding:
                    HandleFindShelf();
                    break;
            }
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            anim.SetBool("Run", false);
        else
            anim.SetBool("Run", true);
    }

    private void HandleIdleRun()
    {
        List<BuildingBase> list = BuildingSystem.Instance.GetShelfBuilding();
        GameUtil.Shuffle(list);
        foreach (BuildingBase shelf in list)
        {
            _ = MoveToDestination(shelf.transform.position,() =>
            {
                GameEntry.Time.CreateTimer(this, 1,()=>
                {
                    CurrentState = WorkerState.GoToProduceBuilding;
                });
            });
            break;
        }
    }
    
    private void HandleFindProduce()
    {
        foreach (BuildingBase shelf in BuildingSystem.Instance.GetpProduceBuilding())
        {
            foreach (var foodSpawner in shelf.foodSpawners)
            {
                if (!WorkerData.collectFood.Contains(foodSpawner.food.foodName)) continue;
                if (foodSpawner.HasSpawnedFood())
                {
                    _ = MoveToDestination(shelf.transform.position + new Vector3(GameUtil.RandomRange(-2f,2f),0,0f),() =>
                    {
                        CurrentState =  WorkerState.GoToShelfBuilding;
                        GameUtil.LogError($"到达Produce");
                    });
                    return;
                }
            }
        }
        GameEntry.Time.CreateTimer(this, 2, HandleFindProduce);
    }

    private void HandleFindShelf()
    {
        if (collectedFood.Count <= 0)
        {
            CurrentState = WorkerState.Idle;
        }
        else
        {
            foreach (BuildingBase shelf in BuildingSystem.Instance.GetShelfBuilding())
            {
                if (collectedFood.All(food => food.foodName != shelf.Entity.Consume)) continue;
                _ = MoveToDestination(shelf.transform.position,() =>
                {
                    CurrentState =  WorkerState.Idle;
                });
                return;
            }
            GameEntry.Time.CreateTimer(this, 2, HandleFindShelf);
        }
    }

    private bool ReachedTargetPos()
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

    public override void HandleOnStay(Collider other)
    {
        if (other.CompareTag("Shelf"))
        {
            BuildingBase building = other.GetComponent<BuildingBase>();
            if (building.CanPlaceFood())
            {
                int collectedFoodCount = collectedFood.Count - 1;
                if (collectedFoodCount >= 0)
                {
                    for (int i = collectedFood.Count - 1; i >= 0; i--)
                    {
                        Food food = collectedFood[i];
                        if (food.foodName == building.Entity.Consume)
                        {
                            removedAnyFood = true;
                            FindObjectOfType<AudioManager>().Play("PlaceFood");
                            building.AddFoodToBuilding(food);
                            ClearCollectFood(food);
                            break;
                        }
                    }

                    if (removedAnyFood)
                    {
                        foodCollectPos.localPosition = initialFoodCollectPos;
                        foreach (Food food in collectedFood)
                        {
                            food.transform.localPosition = foodCollectPos.localPosition;
                            foodCollectPos.localPosition = new Vector3(foodCollectPos.transform.localPosition.x, foodCollectPos.transform.localPosition.y + 1, foodCollectPos.transform.localPosition.z);
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
