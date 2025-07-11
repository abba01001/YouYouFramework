using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using DG.Tweening;

public class Customer : MonoBehaviour
{
    private GameObject[] availableShelfs;
    public Transform[] trollyPoses;
    //[HideInInspector]
    public List<Food>collectedFoods;
    public int buyFoodCapacity, trollyPoseCount = 0;
    private bool goToBillingCounter, canCollect = true;
    [HideInInspector]
    public bool counterLook;
    private BillingDesk billingDesk;
    public Transform handPos, target;
    [HideInInspector]
    public Transform exitTransform;
    public GameObject moneyPrefab, trolly;
    public MeshFilter hat;
    public SkinnedMeshRenderer skin;
    [HideInInspector]
    public NavMeshAgent agent;
    public Animator anim;
    Vector3 targetShelfPos;
    private CustomerPoints _CustomerPoints;

    private void Start()
    {
        GameManager _GameManager = FindObjectOfType<GameManager>();
        skin.material.color = _GameManager.customerColors[Random.Range(0, _GameManager.customerColors.Length)];
        hat.mesh = _GameManager.customerHats[Random.Range(0, _GameManager.customerHats.Length)];

        billingDesk = FindObjectOfType<BillingDesk>();

        buyFoodCapacity = Random.Range(1, 4);

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;

        availableShelfs = GameObject.FindGameObjectsWithTag("Shelf");

        targetShelfPos = FindShelf();

        agent.SetDestination(targetShelfPos);
    }

    private Vector3 FindShelf()
    {
        int randVal = Random.Range(0, availableShelfs.Length);

        foreach (CustomerPoints customerPoint in availableShelfs[randVal].GetComponent<FoodPlaceManager>().customerPoints)
        {
            if (!customerPoint.fill)
            {
                customerPoint.fill = true;
                _CustomerPoints = customerPoint;
                return customerPoint.transform.position;
            }
        }

        return FindShelf();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Exit"))
        {
            other.GetComponentInParent<CustomerSpawner>().SpawnCustomer();
            Destroy(this.gameObject);
        }

        if (other.CompareTag("CustomerPoint") && !goToBillingCounter)
        {
            if (other.gameObject == _CustomerPoints.gameObject)
            {
                agent.updateRotation = false;
                transform.rotation = other.transform.rotation;
            }
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("CustomerPoint"))
    //    {
    //        print("Name  " + other.gameObject);

    //        if (other.gameObject == _CustomerPoints.gameObject)
    //            _CustomerPoints.fill = false;
    //    }
    //}

    public void GoToBillingCounter()
    {
        goToBillingCounter = true;
        agent.updateRotation = true;
        _CustomerPoints.fill = false;
        agent.isStopped = false;
        billingDesk.customersForBilling.Add(this);
        billingDesk.ArrangeCustomersInQue();
    }

    public void GoToExit()
    {
        
        billingDesk.customersForBilling.Remove(this);
        agent.SetDestination(exitTransform.position);
        billingDesk.ArrangeCustomersInQue();
    }

    public void PayMoney()
    {
        int val = buyFoodCapacity * 2;

        for (int i = 0; i < val; i++) 
        {
            int index = billingDesk.moneyPosCount;

            GameObject money = Instantiate(moneyPrefab, transform.position, transform.rotation);

            money.transform.DOJump(billingDesk.moneyPos[index].position, 4, 1, .4f)
            .OnComplete(delegate ()
            {
                billingDesk.money.Add(money);
            });

            if (billingDesk.moneyPosCount == 9) 
            {
                billingDesk.moneyPosCount = 0;

                Vector3 vec = billingDesk.moneyPosParent.position;
                vec.y = vec.y+1;

                billingDesk.moneyPosParent.position = vec;
            }
            else
                billingDesk.moneyPosCount++;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Shelf") && ReachedDestinationOrGaveUp())
        {
            FoodPlaceManager shelf = other.GetComponent<FoodPlaceManager>();

            if (shelf.collectedFoods.Count > 0 && canCollect)
            {
                Food food = shelf.collectedFoods[shelf.collectedFoods.Count - 1];
                shelf.collectedFoods.Remove(food);
                shelf.MoveShelfTopTransform();

                food.GotoCustomer(trollyPoses[trollyPoseCount], this);
                collectedFoods.Add(food);
                
                trollyPoseCount++;
                canCollect = false;
            }
        }
    }

    public void FoodColected()
    {
       if (collectedFoods.Count == buyFoodCapacity)
       {
           Invoke("GoToBillingCounter", 0.5f);
           return;
       }
       canCollect = true;       
    }

    private void Update()
    {
        if (counterLook)
        {
            if (ReachedDestinationOrGaveUp())
            {
                transform.rotation = target.rotation;
                counterLook = false;
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
}
