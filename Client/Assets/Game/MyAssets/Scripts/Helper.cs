using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Helper : MonoBehaviour
{
    public Transform foodCollectPos;
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

    private void Start()
    {
        trashBin = GameObject.FindGameObjectWithTag("TrashBin").transform;

        _PlayerManager.maxFoodPlayerCarry = PlayerPrefs.GetInt("CapacityVal", _PlayerManager.maxFoodPlayerCarry);

        GetComponent<NavMeshAgent>().speed = PlayerPrefs.GetFloat("SpeedVal", GetComponent<NavMeshAgent>().speed);

        initialFoodCollectPos = foodCollectPos.transform.localPosition;

        agent.updateRotation = true;

        FindShelf();
    }

    private void FindShelf()
    {
        shelfs = GameObject.FindGameObjectsWithTag("Shelf");

        foreach (GameObject shelf in shelfs)
        {
            FoodPlaceManager _FoodPlaceManager = shelf.GetComponent<FoodPlaceManager>();

            int i = _FoodPlaceManager.collectFoodCapacity / 2;

            if (_FoodPlaceManager.collectedFoods.Count < i)
            {
                targetShelfPos = _FoodPlaceManager.HelperPos;

                foreach (FoodSpawner foodSpawner in _FoodPlaceManager.availableFoodSpawners)
                {
                    if (foodSpawner.food.foodName == _FoodPlaceManager.shelfFoodName)
                    {

                        if (foodSpawner.foodObj != null)
                        {
                            _PlayerManager.currentFoodName = _FoodPlaceManager.shelfFoodName;

                            Goto(foodSpawner.transform.position);
                            return;
                        }
                    }

                }
            }
        }

        Invoke("FindShelf", 2);
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
            if (_PlayerManager.collectedFood.Count >= _PlayerManager.minimumFoodPlayerCarry)
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
                Invoke("FindShelf", .5f);
            }
            else
            {
                agent.SetDestination(trashBin.position);
            }
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
                        if (_PlayerManager.collectedFood[i].foodName == shelf.shelfFoodName)
                        {
                            removedAnyFood = true;
                            _PlayerManager.collectedFood[i].PlaceFood(shelf.shelfTopTransform);
                            FindObjectOfType<AudioManager>().Play("PlaceFood");

                            shelf.collectedFoods.Add(_PlayerManager.collectedFood[i]);
                            _PlayerManager.collectedFood[i].transform.parent = shelf.transform;
                            shelf.MoveShelfTopTransform();

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
        }
    }

    public void IncreaseCapacity(int increaseVal)
    {
        _PlayerManager.maxFoodPlayerCarry += increaseVal;

        PlayerPrefs.SetInt("CapacityVal", _PlayerManager.maxFoodPlayerCarry);
    }

    public void IncreaseSpeed(int increaseVal)
    {
        GetComponent<NavMeshAgent>().speed += increaseVal;

        PlayerPrefs.SetFloat("SpeedVal", GetComponent<NavMeshAgent>().speed);
    }
}
