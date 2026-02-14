using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using YouYou;

public class Helper : CharacterBase
{
    private Transform foodCollectPos;
    private Vector3 initialFoodCollectPos;
    private bool removedAnyFood;
    public Animator anim;
    public ParticleSystem upgradeParticle;
    private Transform trashBin;
    private GameObject[] shelfs;
    
    public override void Init(CharacterData data)
    {
        base.Init(data);
        foodCollectPos = transform.Find("FoodCollectPos");
        GetComponent<NavMeshAgent>().speed = CharacterData.speed;
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
                case CharacterState.Idle:
                    HandleIdleRun();
                    break;
                case CharacterState.GoToProduceBuilding:
                    HandleFindProduce();
                    break;
                case CharacterState.GoToShelfBuilding:
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
        _ = MoveToDestination(transform.position + new Vector3(GameUtil.RandomRange(-10f,10f),0,GameUtil.RandomRange(-10f,10f)),() =>
        {
            GameEntry.Time.CreateTimer(this, 1,()=>
            {
                CurrentState = CharacterState.GoToProduceBuilding;
            });
        });
    }
    
    private void HandleFindProduce()
    {
        foreach (BuildingBase shelf in BuildingSystem.Instance.GetpProduceBuilding())
        {
            foreach (var foodSpawner in shelf.foodSpawners)
            {
                if (!CharacterData.collectFood.Contains(foodSpawner.food.foodName)) continue;
                if (foodSpawner.HasSpawnedFood())
                {
                    _ = MoveToDestination(shelf.transform.position + new Vector3(GameUtil.RandomRange(-2f,2f),0,0f),() =>
                    {
                        CurrentState =  CharacterState.GoToShelfBuilding;
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
            CurrentState = CharacterState.Idle;
        }
        else
        {
            foreach (BuildingBase shelf in BuildingSystem.Instance.GetShelfBuilding())
            {
                if (collectedFood.All(food => food.foodName != shelf.Entity.Consume)) continue;
                _ = MoveToDestination(shelf.transform.position,() =>
                {
                    CurrentState =  CharacterState.Idle;
                });
                return;
            }
            GameEntry.Time.CreateTimer(this, 2, HandleFindShelf);
        }
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
        CharacterData.maxFoodCarry += increaseVal;
    }

    public void IncreaseSpeed(int increaseVal)
    {
        GetComponent<NavMeshAgent>().speed += increaseVal;
        CharacterData.speed = GetComponent<NavMeshAgent>().speed;
    }
}
