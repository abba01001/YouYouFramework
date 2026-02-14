using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using YouYou;

public class Player : CharacterBase
{
    private BillingDesk _BillingDesk;
    private bool removedAnyFood;
    public PlayerManager _PlayerManager;
    public int playerCapacityBuyAmount;
    public Text playerCapaciyTest;
    private Transform foodCollectPos;
    private Vector3 initialFoodCollectPos;

    private void Start()
    { 
        _BillingDesk = FindObjectOfType<BillingDesk>();
        Init(GameEntry.Data.PlayerRoleData.restaurantData.player);
    }

    public override void Init(CharacterData data)
    {
        base.Init(data);
        foodCollectPos = transform.Find("FoodCollectPos");
        initialFoodCollectPos = foodCollectPos.transform.localPosition;
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
        
        if (other.CompareTag("BillingDeskCollider"))
        {
            if (_BillingDesk.money.Count > 0)
            {
                foreach (GameObject money in _BillingDesk.money)
                {
                    money.transform.DOJump(transform.position, 4, 1, .4f)
                        .OnComplete(delegate ()
                        {
                            GameEntry.Data.AddMoney(5);
                            AudioManager.Instance.Play("MoneyCollect");
                            Destroy(money);
                        });
                }

                _BillingDesk.money = new List<GameObject>();
                _BillingDesk.moneyPosCount = 0;

                Vector3 vec = _BillingDesk.moneyPosParent.position;
                vec.y = 2;

                _BillingDesk.moneyPosParent.position = vec;
            }
        }
    }

    public override void HandleOnEnter(Collider other)
    {
        base.HandleOnEnter(other);
        if (other.gameObject.CompareTag("HelperSpawner"))
        {
            other.GetComponent<HelperBuy_UpgradePoint>().OpenWindow();
        }
    }

    public override void HandleOnExit(Collider other)
    {
        base.HandleOnExit(other);
        if (other.gameObject.CompareTag("HelperSpawner"))
        {
            other.GetComponent<HelperBuy_UpgradePoint>().CloseWindow();
        }
    }

    public void IncreasePlayerCapacity()
    {
        if (GameEntry.Data.Coin >= playerCapacityBuyAmount)
        {
            AudioManager.Instance.Play("Upgrade");

            CharacterData.maxFoodCarry++;
            PlayerPrefs.SetInt("PlayerCapacity",CharacterData.maxFoodCarry);

            playerCapacityBuyAmount += 100;
            PlayerPrefs.SetInt("PlayerCapacityBuyAmount", playerCapacityBuyAmount);

            // playerCapaciyTest.text = playerCapacityBuyAmount.ToString();
        }
    }

    public override void Tick()
    {
        
    }
}
