using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using DG.Tweening;
using YouYou;
using Random = UnityEngine.Random;

public enum CustomerMood
{
    Happy,   // 高兴的顾客，购物效率高，买得更多
    Neutral, // 中立的顾客，购物行为正常
    Annoyed, // 不满的顾客，购物效率低，可能放弃购物
    Anxious,  // 焦虑的顾客，购物效率低，反复挑选商品
    Angry //愤怒的顾客，离开购物
}

public class Customer : MonoBehaviour
{
    private List<Transform> slotList = new List<Transform>();
    private int tempSlotId = 0;
    
    private bool isGoToCollect = false;
    private bool goToBillingCounter, canCollect = true;
    public bool counterLook;
    private BillingDesk billingDesk;
    public Transform handPos, target;
    public GameObject moneyPrefab, trolly;
    public MeshFilter hat;
    public SkinnedMeshRenderer skin;
    public NavMeshAgent agent;
    public Animator anim;
    public GameObject packageObj;
    private CustomerPoints _CustomerPoints;
    private bool hasChangeNeutral;
    public bool IsExit { get; set; }
    public CustomerData CustomerData{ get; set; }
    public bool IsActive{ get; set; }
    private int ShoppingTime { get; set; }
    private TimeAction modTimeAction;
    public CustomerMood currentMood; // 默认情绪是中立
    public List<Food>collectedFoods;
    private bool isCollecting;
    public void Init(CustomerData data)
    {
        packageObj.MSetActive(false);
        trolly.MSetActive(true);
        isGoToCollect = false;
        tempSlotId = 0;
        goToBillingCounter = false;
        canCollect = true;
        _CustomerPoints = null;
        ShoppingTime = 0;
        collectedFoods.Clear();
        CustomerData = data;
        slotList.Clear();
        foreach (Transform child in transform.Find("TrolleySlots"))
        {
            slotList.Add(child);
        }
        
        skin.material.color = GameEntry.Instance.customerColors[data.hatColorIndex];
        hat.mesh = GameEntry.Instance.customerHats[data.meshColorIndex];
        currentMood = data.mood;
        billingDesk = FindObjectOfType<BillingDesk>();

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        IsExit = false;
        IsActive = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CustomerPoint") && !goToBillingCounter)
        {
            if (other.gameObject == _CustomerPoints.gameObject)
            {
                agent.updateRotation = false;
                StartCoroutine(SmoothRotate(other.transform.rotation));
                modTimeAction ??= GameEntry.Time.CreateTimerLoop(this, 1, -1, (_) => { ShoppingTime += 1; });
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CustomerPoint") && !goToBillingCounter)
        {
            if (other.gameObject == _CustomerPoints.gameObject)
            {
                _CustomerPoints.fill = false;
            }
        }
    }
    
    private IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        float timeElapsed = 0f;
        float rotationDuration = .2f; // 旋转的时间，单位为秒
        Quaternion initialRotation = transform.rotation;
        while (timeElapsed < rotationDuration)
        {
            transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, timeElapsed / rotationDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation; // 最终确保旋转到目标值
    }
    
    private void UpdateMoodBasedOnTime()
    {
        if (modTimeAction == null) return;
        if (ShoppingTime <= CustomerData.happyTime)
        {
            currentMood = CustomerMood.Happy;
        }
        else if (ShoppingTime <= CustomerData.neutralTime && !hasChangeNeutral)
        {
            currentMood = CustomerMood.Neutral;
            hasChangeNeutral = true;
        }
        else if (ShoppingTime <= CustomerData.annoyedTime)
        {
            currentMood = CustomerMood.Annoyed;
        }
        else if (ShoppingTime <= CustomerData.anxiousTime)
        {
            currentMood = CustomerMood.Anxious;
        }
        else if (ShoppingTime <= CustomerData.angryTime)
        {
            currentMood = CustomerMood.Angry;
        }
    }

    private void CheckModChange()
    {
        if (IsExit) return;
        if (modTimeAction == null) return;
        UpdateMoodBasedOnTime();
        UpdateMoodBasedOnStoreConditions();
        if (currentMood == CustomerMood.Anxious)
        {
        }
        if (currentMood == CustomerMood.Angry)
        {
            GoToExit();
        }
    }
    
    // 更新顾客情绪基于商店条件（只在 Neutral 时生效）
    private void UpdateMoodBasedOnStoreConditions()
    {
        if (modTimeAction == null) return;
        if (currentMood == CustomerMood.Neutral)
        {
            if (CustomerSystem.Instance.IsStoreBusy())
            {
                currentMood = CustomerMood.Anxious; // 商店拥挤导致焦虑
            }
            else if (BuildingSystem.Instance.HasPromotionActivity())
            {
                currentMood = CustomerMood.Happy; // 促销活动导致高兴
            }
        }
    }

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
        agent.updateRotation = true;
        modTimeAction?.Stop();
        modTimeAction = null;
        billingDesk.customersForBilling.Remove(this);
        int index = GameUtil.RandomRange(0, CustomerSystem.Instance.bornPos.Count);
        Vector3 tarGetPos = CustomerSystem.Instance.bornPos[index];
        agent.SetDestination(tarGetPos);
        _CustomerPoints.fill = false;
        GameEntry.Time.CreateTimer(this, 1f, () =>
        {
            IsExit = true;
        });
        billingDesk.ArrangeCustomersInQue();
    }

    public void PayMoney()
    {
        int val = 10 * 2;

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
        BuildingBase building = other.gameObject.GetComponent<BuildingBase>();
        if (building != null && building.Entity.BuildingType == "Shelf" && ReachedDestinationOrGaveUp())
        {
            if (canCollect)
            {
                FoodPlaceManager shelf = other.GetComponent<FoodPlaceManager>();
                if (shelf.collectedFoods.Count > 0)
                {
                    Food food = shelf.collectedFoods[shelf.collectedFoods.Count - 1];
                    shelf.collectedFoods.Remove(food);
                    shelf.MoveShelfTopTransform();

                    CollectFood(building.Entity.Consume);
                    food.GotoCustomer(slotList[tempSlotId], ()=>
                    {
                        ResetCanCollect(building.Entity.Consume);
                    });
                    collectedFoods.Add(food);
                    tempSlotId++;
                    canCollect = false;
                }
            }

        }
    }

    private void CollectFood(string name)
    {
        foreach (var data in CustomerData.foodList)
        {
            if (data.name == name)
            {
                data.hasCount += 1;
                break;
            }
        }
        CustomerSystem.Instance.PrintFoordData(CustomerData);
    }

    public void ResetCanCollect(string foodName)
    {
        if (CustomerSystem.Instance.CheckIsFullCollect(CustomerData,foodName))
        {
            isGoToCollect = false;
            return;
        }
        canCollect = true;
    }


    // 更新所有顾客
    public void OnUpdate()
    {
        if (!IsActive) return;
        if (agent == null) return;
        
        CheckModChange();
        CheckGoToCollect();
        if (counterLook)
        {
            if (ReachedDestinationOrGaveUp())
            {
                transform.rotation = target.rotation;
                counterLook = false;
            }
        }
        
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            anim.SetBool("Run", false);
            if (IsExit)
            {
                CustomerSystem.Instance.RemoveCustomer(this);
            }
        }
        else
        {
            anim.SetBool("Run", true);
        }
    }

    private void CheckGoToCollect()
    {
        if (IsExit) return;
        if (!isGoToCollect)
        {
            if (CustomerSystem.Instance.CheckIsFullCollect(CustomerData))
            {
                GoToBillingCounter();
                isGoToCollect = true;
                return;
            }

            foreach (FoodData data in CustomerData.foodList)
            {
                if (data.needCount == data.hasCount) continue;
                GameObject obj = BuildingSystem.Instance.GetShelfBuilding(data.name);

                if (obj != null)
                {
                    foreach (var customerPoint in obj.GetComponent<FoodPlaceManager>().customerPoints)
                    {
                        if (!customerPoint.fill)
                        {
                            customerPoint.customerObj = transform.gameObject.name;
                            customerPoint.fill = true;
                            _CustomerPoints = customerPoint;
                            agent.SetDestination(customerPoint.transform.position);
                            agent.updateRotation = true;
                            isGoToCollect = true;
                            break;
                        }
                    }
                }
                if (isGoToCollect) break;
            }
        }
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

    public void DoPlayPackageAnim(Action cb)
    {
                    
        trolly.MSetActive(false);
        packageObj.transform.DOJump(handPos.position, 4, 1, .3f)
            .OnComplete(delegate ()
            {
                packageObj.transform.position = handPos.position;
                packageObj.transform.rotation = handPos.rotation;
                packageObj.transform.parent = transform;
                PayMoney();
                cb?.Invoke();
            });
    }

    public void InitPackagBox(Vector3 pos,Quaternion rotation)
    {
        packageObj.transform.position = pos;
        packageObj.transform.rotation = rotation;
        packageObj.gameObject.MSetActive(true);
        packageObj.transform.parent = null;
    }
}
