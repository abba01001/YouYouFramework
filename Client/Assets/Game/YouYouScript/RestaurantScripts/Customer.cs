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


public class Customer : CharacterBase
{
    [SerializeField] private Transform panel;
    private List<Transform> slotList = new List<Transform>();
    
    private BillingDesk billingDesk;
    public Transform handPos;

    public GameObject moneyPrefab, trolly;
    public MeshFilter hat;
    public SkinnedMeshRenderer skin;
    public Animator anim;
    public GameObject packageObj;
    public CustomerPoint tempCustomerPoint;

    public SpriteRenderer foodSpriteBg;
    public List<SpriteRenderer> foodSprites;

    private bool fullCollectFlag = false;
    public override void Init(CharacterData data)
    {
        base.Init(data);
        
        packageObj.MSetActive(false);
        trolly.MSetActive(true);
        fullCollectFlag = false;
        tempCustomerPoint?.Clean();
        tempCustomerPoint = null;
        slotList.Clear();
        foreach (Transform child in transform.Find("TrolleySlots"))
        {
            slotList.Add(child);
        }
        
        skin.material.color = GameEntry.Instance.customerColors[data.hatColorIndex];
        hat.mesh = GameEntry.Instance.customerHats[data.meshColorIndex];
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        IsExit = false;
        IsLiving = true;
        RefresFoodSprite(true);
    }

    public void RefresFoodSprite(bool init = false)
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
        foreach (var f in CharacterData.foodList)
        {
            if (index >= foodSprites.Count) break;
            foodSprites[index].transform.parent.gameObject.MSetActive(true);
            foodSprites[index].transform.parent.GetComponentInChildren<TextMeshPro>(true).text = $"{f.hasCount}/{f.needCount}";
            index++;
        }
        foodSpriteBg.size = new Vector2(0.8f,index == 1 ? 0.45f : index == 2 ? 0.9f : index == 3 ? 1.25f : 1.65f);
    }

    
    private void SmoothRotate(Vector3 targetRotation,float duration = 0.2f)
    {
        transform.DORotate(targetRotation, 0.2f);
    }

    public void GoToExit()
    {
        agent.updateRotation = true;
        billingDesk.RemoveCustomer(this);
        int index = GameUtil.RandomRange(0, CustomerSystem.Instance.bornPos.Count);
        Vector3 tarGetPos = CustomerSystem.Instance.bornPos[index];
        agent.SetDestination(tarGetPos);
        tempCustomerPoint?.Clean();
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



    public void CollectFood(Food food)
    {
        foreach (var data in CharacterData.foodList)
        {
            if (data.name == food.name)
            {
                data.hasCount += 1;
                collectedFood.Add(food);
                food.GotoCustomer(slotList[collectedFood.Count]);
                RefresFoodSprite();
                break;
            }
        }
        CustomerSystem.Instance.PrintFoordData(CharacterData);
    }


    // 更新所有顾客
    public override void Tick()
    {
        if (!IsLiving) return;
        HandlePanelToCamera();

        
        if (isStateChanged)
        {
            isStateChanged = false;
            switch (CurrentState)
            {
                case CharacterState.Idle:
                    HandleIdleRun();
                    break;
                case CharacterState.GoToShelfBuilding:
                    HandleFindShelf();
                    break;
                case CharacterState.GoToBillingDesk:
                    HandleFindBillingDesk();
                    break;
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

    private void HandleIdleRun()
    {
        _ = MoveToDestination(transform.position + new Vector3(GameUtil.RandomRange(-10f,10f),0,GameUtil.RandomRange(-10f,10f)),() =>
        {
            GameEntry.Time.CreateTimer(this, 1,()=>
            {
                CurrentState = CharacterState.GoToShelfBuilding;
            });
        });
    }

    private void HandleFindShelf()
    {
        CustomerPoint point = GetTargetCustomerPoint();
        if (point != null && !CustomerSystem.Instance.CheckPointIsOccupied(point))
        {
            tempCustomerPoint = point;
            tempCustomerPoint.customerName = transform.gameObject.name;
            tempCustomerPoint.fill = true;
            
            agent.updateRotation = true;
            _ = MoveToDestination(tempCustomerPoint.transform.position, () =>
            {
                agent.updateRotation = false;
                SmoothRotate(tempCustomerPoint.transform.rotation.eulerAngles);
            });
            return;
        }
        GameEntry.Time.CreateTimer(this, 2, HandleFindShelf);
    }

    private void HandleFindBillingDesk()
    {
        billingDesk = BuildingSystem.Instance.GetBillingDesk();
        agent.updateRotation = true;
        agent.isStopped = false;
        billingDesk.AddCustomer(this);
        
        
        tempCustomerPoint?.Clean();
    }
    
    
    public void GoToBillingCounter()
    {

    }


    private void HandlePanelToCamera()
    {
        Vector3 directionToFace = Camera.main.transform.position - panel.position;
        directionToFace.y = 0;
        panel.rotation = Quaternion.LookRotation(-directionToFace);
    }

    public void SetFullCollectFlag()
    {
        tempCustomerPoint?.Clean();
        tempCustomerPoint = null;
        fullCollectFlag = true;
        CurrentState = CharacterState.GoToBillingDesk;
    }

    public CustomerPoint GetTargetCustomerPoint()
    {
        foreach (FoodData data in CharacterData.foodList)
        {
            if (data.needCount == data.hasCount) continue;
            BuildingBase building = BuildingSystem.Instance.GetShelfBuilding(data.name);
            if (building == null) continue;
            CustomerPoint point = building.GetIdlePoint();
            if (point == null) continue;
            return point;
        }
        return null;
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
        while (collectedFood.Count > 0)
        {
            int foodCount = collectedFood.Count;
            Food food = collectedFood[foodCount - 1];
            collectedFood.RemoveAt(foodCount - 1);
            await food.GotoBillingCounterBox(billingDesk.packageBoxPos);
            Destroy(food.gameObject);
        }
        CharacterData.foodList.Clear();
        packageObj.GetComponent<Animator>().SetTrigger("StartProduction");
        await UniTask.Delay(600);
        await DoPlayPackageAnim();
    }

    public bool IsNullFood()
    {
        return collectedFood.Count == 0;
    }

    public void CheckGotoBillingDesk(Vector3 pos)
    {
        MoveToDestination(pos,()=>
        {
             SmoothRotate(new Vector3(0,-90,0));
        });
    }
}
