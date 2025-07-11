using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Farmer : MonoBehaviour
{
    private Vector3 initialFoodCollectPos;
    public Transform foodCollectPos;
    private Transform standPos;
    public NavMeshAgent agent;
    private Transform targetChickenPos;
    private bool canCheck = true;
    private bool reachedShelf;
    public Animator anim;
    private bool removedAnyFood;

    public PlayerManager _PlayerManager;
    private System.Random _random = new System.Random();

    private void Start()
    {
        standPos = GameObject.Find("StandPos").transform;

        initialFoodCollectPos = foodCollectPos.transform.localPosition;

        agent.updateRotation = true;

        FindChicken();
    }

    private GameObject[] chickens;

    private void FindChicken()
    {
        chickens = GameObject.FindGameObjectsWithTag("Chicken");

        for(int i =0; i< chickens.Length; i++)
        {
            FoodPlaceManager chicken = chickens[i].GetComponent<FoodPlaceManager>();

            int j = chicken.collectFoodCapacity / 2;

            if (chicken.collectedFoods.Count < j)
            {
                targetChickenPos = chicken.HelperPos;
                FindFoodSpawner(chicken.shelfFoodName);
                return;
            }
        }

        Invoke("FindChicken", 2);
    }
    private void FindFoodSpawner(string _foodName)
    {
        FoodSpawner[] availableFoodSpawners = FindObjectsOfType<FoodSpawner>();

        Shuffle(availableFoodSpawners);

        foreach (FoodSpawner foodSpawner in availableFoodSpawners)
        {
            if (foodSpawner.food.foodName == _foodName)
            {
                Goto(foodSpawner.transform.position);
                return;
            }
        }

        FindChicken();
    }

    void Shuffle(FoodSpawner[] array)
    {
        int p = array.Length;
        for (int n = p - 1; n > 0; n--)
        {
            int r = _random.Next(0, n);
            FoodSpawner t = array[r];
            array[r] = array[n];
            array[n] = t;
        }
    }

    private void Goto(Vector3 target)
    {
        agent.SetDestination(target);
        canCheck = true;
    }

    private void Update()
    {
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
            Invoke("FindChicken", 3);
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            anim.SetBool("Run", false);
        else
            anim.SetBool("Run", true);
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

    private void OnTriggerStay(Collider other)
    {
        print("Collided  " + other.gameObject.name);

        if (other.CompareTag("Chicken"))
        {
            FoodPlaceManager ChickenShelf = other.GetComponent<FoodPlaceManager>();

            if (ChickenShelf.collectedFoods.Count < ChickenShelf.collectFoodCapacity)
            {
                int collectedFoodCount = _PlayerManager.collectedFood.Count - 1;

                if (collectedFoodCount >= 0)
                {
                    for (int i = _PlayerManager.collectedFood.Count - 1; i >= 0; i--)
                    {
                        if (_PlayerManager.collectedFood[i].foodName == ChickenShelf.shelfFoodName)
                        {
                            removedAnyFood = true;
                            _PlayerManager.collectedFood[i].PlaceFood(ChickenShelf.shelfTopTransform);
                            FindObjectOfType<AudioManager>().Play("PlaceFood");

                            ChickenShelf.collectedFoods.Add(_PlayerManager.collectedFood[i]);
                            _PlayerManager.collectedFood[i].transform.parent = ChickenShelf.transform;
                            ChickenShelf.MoveShelfTopTransform();

                            _PlayerManager.collectedFood[i].goToCustomer = true;
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
                Goto(standPos.position);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chicken"))
            reachedShelf = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Chicken"))
            reachedShelf = false;
    }
}
