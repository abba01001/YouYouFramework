using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AI;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
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
    [SerializeField] private Transform panel;
    private List<Transform> slotList = new List<Transform>();
    
    private bool isCollecting = false;
    private bool IsBilling = true;
    public bool counterLook;
    private BillingDesk billingDesk;
    public Transform handPos;

    private bool isInCashier;
    public bool IsInCashier
    {
        get => isInCashier;
        set
        {
            isInCashier = value;
            counterLook = true;
        }
    }

    public GameObject moneyPrefab, trolly;
    public MeshFilter hat;
    public SkinnedMeshRenderer skin;
    public NavMeshAgent agent;
    public Animator anim;
    public GameObject packageObj;
    public CustomerPoints _CustomerPoints;
    private bool hasChangeNeutral;
    public bool IsExit { get; set; }
    public CustomerData CustomerData{ get; set; }
    public bool IsLiving{ get; set; }
    private int ShoppingTime { get; set; }
    private TimeAction modTimeAction;
    public CustomerMood currentMood; // 默认情绪是中立
    public List<Food>collectedFoods;
    private TriggerDetector triggerDetector;

    public SpriteRenderer foodSpriteBg;
    public List<SpriteRenderer> foodSprites; 

    public void Init(CustomerData data)
    {
        IsInCashier = false;
        if (triggerDetector == null)
        {
            triggerDetector = gameObject.AddComponent<TriggerDetector>();
        }
        else
        {
            triggerDetector.UnsubscribeAll();
        }
        
        // 绑定触发器事件
        triggerDetector.OnEnter += HandleOnEnter;
        triggerDetector.OnStay += HandleOnStay;
        triggerDetector.OnExit += HandleOnExit;
        
        foreach (var f in collectedFoods.ToList())
        {
            Destroy(f.gameObject);
        }
        collectedFoods.Clear();
        packageObj.MSetActive(false);
        trolly.MSetActive(true);
        isCollecting = false;
        IsBilling = false;
        _CustomerPoints?.Clean();
        _CustomerPoints = null;
        ShoppingTime = 0;
        CustomerData = data;
        slotList.Clear();
        foreach (Transform child in transform.Find("TrolleySlots"))
        {
            slotList.Add(child);
        }
        
        skin.material.color = GameEntry.Instance.customerColors[data.hatColorIndex];
        hat.mesh = GameEntry.Instance.customerHats[data.meshColorIndex];
        currentMood = data.mood;
        billingDesk = BuildingSystem.Instance.GetBillingDesk();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        IsExit = false;
        IsLiving = true;
        RefresFoodSprite(true);
    }

    private void RefresFoodSprite(bool init = false)
    {
        if (init)
        {
            foreach (var s in foodSprites)
            {
                s.transform.parent.gameObject.MSetActive(false);
                s.SetSpriteByAtlas(Constants.AtlasPath.Game,$"icon-tomato", true);
            }
        }

        int index = 0;
        foreach (var f in CustomerData.foodList)
        {
            if (index >= foodSprites.Count) break;
            foodSprites[index].transform.parent.gameObject.MSetActive(true);
            foodSprites[index].transform.parent.GetComponentInChildren<TextMeshPro>(true).text = $"{f.hasCount}/{f.needCount}";
            index++;
        }
        foodSpriteBg.size = new Vector2(0.8f,index == 1 ? 0.45f : index == 2 ? 0.9f : index == 3 ? 1.25f : 1.65f);
    }


    private void HandleOnEnter(Collider other)
    {
        if (other.CompareTag("CustomerPoint") && !IsBilling)
        {
            if (other.gameObject == _CustomerPoints.gameObject)
            {
                agent.updateRotation = false;
                SmoothRotate(other.transform.rotation.eulerAngles);
                modTimeAction ??= GameEntry.Time.CreateTimerLoop(this, 1, -1, (_) => { ShoppingTime += 1; });
            }
        }
    }
    
    private void HandleOnStay(Collider other)
    {
        BuildingBase building = other.gameObject.GetComponent<BuildingBase>();
        if (building != null && building.Entity.BuildingType == "Shelf" && ReachedDestinationOrGaveUp())
        {
            FoodPlaceManager shelf = other.GetComponent<FoodPlaceManager>();
            if (CustomerSystem.Instance.CheckIsFullCollect(CustomerData,shelf.shelfFoodName))
            {
                isCollecting = false;
                _CustomerPoints?.Clean();
                _CustomerPoints = null;
                return;
            }
            if (shelf.collectedFoods.Count > 0)
            {
                Food food = shelf.collectedFoods[shelf.collectedFoods.Count - 1];
                shelf.collectedFoods.Remove(food);

                CollectFood(building.Entity.Consume);
                food.GotoCustomer(slotList[collectedFoods.Count]);
                collectedFoods.Add(food);
                RefresFoodSprite();
            }

        }
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
    
    private void HandleOnExit(Collider other)
    {
        if (_CustomerPoints != null && other.CompareTag("CustomerPoint") && !IsBilling)
        {
            if (other.gameObject == _CustomerPoints.gameObject)
            {
                _CustomerPoints?.Clean();
            }
        }
    }
    
    private void SmoothRotate(Vector3 targetRotation)
    {
        transform.DORotate(targetRotation, .2f).OnComplete(() =>
        {
        });
    }

    public void GoToBillingCounter()
    {
        IsBilling = true;
        modTimeAction?.Stop();
        modTimeAction = null;
        agent.updateRotation = true;
        _CustomerPoints?.Clean();
        agent.isStopped = false;
        billingDesk.AddCustomer(this);
    }

    public void GoToExit()
    {
        agent.updateRotation = true;
        modTimeAction?.Stop();
        modTimeAction = null;
        billingDesk.RemoveCustomer(this);
        int index = GameUtil.RandomRange(0, CustomerSystem.Instance.bornPos.Count);
        Vector3 tarGetPos = CustomerSystem.Instance.bornPos[index];
        agent.SetDestination(tarGetPos);
        _CustomerPoints?.Clean();
        GameEntry.Time.CreateTimer(this, 0.1f, () =>
        {
            IsExit = true;
        });
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


    // 更新所有顾客
    public void Tick()
    {
        if (!IsLiving) return;
        if (agent == null) return;
        CheckModChange();
        HandlePanelToCamera();
        CheckGoToCollect();
        if (counterLook)
        {
            if (ReachedDestinationOrGaveUp())
            {
                if (IsInCashier)
                {
                    SmoothRotate(new Vector3(0,-90,0));
                }
                // transform.rotation = Quaternion.Euler(0,-90,0);
                counterLook = false;
            }
        }
        
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            anim.SetBool("Run", false);
            if (IsExit) IsLiving = false;
        }
        else
        {
            anim.SetBool("Run", true);
        }
    }

    private void HandlePanelToCamera()
    {
        Vector3 directionToFace = Camera.main.transform.position - panel.position;
        directionToFace.y = 0;
        panel.rotation = Quaternion.LookRotation(-directionToFace);
    }

    private void CheckGoToCollect()
    {
        if (IsExit) return;
        if (IsBilling) return;
        if (CustomerSystem.Instance.CheckIsFullCollect(CustomerData))
        {
            GoToBillingCounter();
            return;
        }
        if (!isCollecting)
        {
            foreach (FoodData data in CustomerData.foodList)
            {
                if (data.needCount == data.hasCount) continue;
                GameObject obj = BuildingSystem.Instance.GetShelfBuilding(data.name);

                if (obj != null)
                {
                    CustomerPoints point = obj.GetComponent<FoodPlaceManager>().GetIdlePoint();
                    if (point != null && !CustomerSystem.Instance.CheckPointIsOccupied(point))
                    {
                        _CustomerPoints = point;
                        _CustomerPoints.customerName = transform.gameObject.name;
                        _CustomerPoints.fill = true;
                        agent.SetDestination(_CustomerPoints.transform.position);
                        agent.updateRotation = true;
                        isCollecting = true;
                        break;
                    }
                }
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

    public async UniTask DoPlayPackageAnim()
    {
        trolly.MSetActive(false);
        packageObj.transform.DOJump(handPos.position, 4, 1, .3f);
        await UniTask.Delay(300);
        packageObj.transform.position = handPos.position;
        packageObj.transform.rotation = handPos.rotation;
        packageObj.transform.parent = transform;
        PayMoney();
    }

    public void InitPackagBox(Transform packageBox)
    {
        packageObj.transform.position = packageBox.position;
        packageObj.transform.rotation = packageBox.rotation;
        packageObj.gameObject.MSetActive(true);
        packageObj.transform.parent = null;
    }

    public async UniTask CheckPackageFood()
    {
        InitPackagBox(billingDesk.packageBoxPos);
        while (collectedFoods.Count > 0)
        {
            int foodCount = collectedFoods.Count;
            Food food = collectedFoods[foodCount - 1];
            collectedFoods.RemoveAt(foodCount - 1);
            await food.GotoBillingCounterBox(billingDesk.packageBoxPos);
            Destroy(food.gameObject);
        }
        CustomerData.foodList.Clear();
        packageObj.GetComponent<Animator>().SetTrigger("StartProduction");
        await UniTask.Delay(600);
        await DoPlayPackageAnim();
    }

    public bool IsNullFood()
    {
        return collectedFoods.Count == 0;
    }

    public void CheckGotoBillingDesk(Vector3 pos)
    {
        agent.SetDestination(pos);
        IsInCashier = true;
    }
}
