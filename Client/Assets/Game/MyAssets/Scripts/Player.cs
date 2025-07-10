using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private GameManager _GameManager;
    private BillingDesk _BillingDesk;
    private bool removedAnyFood;
    public PlayerManager _PlayerManager;
    public int playerCapacityBuyAmount;
    public Text playerCapaciyTest;

    private void Start()
    { 
        _PlayerManager.maxFoodPlayerCarry = PlayerPrefs.GetInt("PlayerCapacity", _PlayerManager.maxFoodPlayerCarry);
        playerCapacityBuyAmount = PlayerPrefs.GetInt("PlayerCapacityBuyAmount", playerCapacityBuyAmount);
        playerCapaciyTest.text = playerCapacityBuyAmount.ToString();

        _GameManager = FindObjectOfType<GameManager>();
        _BillingDesk = FindObjectOfType<BillingDesk>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<FoodPlaceManager>())
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
                            AudioManager.Instance.Play("FoodPlace");

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
                        Transform foodCollectPos = _PlayerManager.foodCollectPos;

                        foodCollectPos.localPosition = _PlayerManager.initialFoodCollectPos;

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

        if (other.CompareTag("BillingDeskCollider"))
        {
            if (_BillingDesk.money.Count > 0)
            {
                foreach (GameObject money in _BillingDesk.money)
                {
                    money.transform.DOJump(transform.position, 4, 1, .4f)
                    .OnComplete(delegate ()
                    {
                        _GameManager.AddMoney(5);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BuyPoint"))
        {
            other.GetComponent<BuyPoint>().StartSpend();
        }

        if (other.gameObject.CompareTag("HelperSpawner"))
        {
            other.GetComponent<HelperBuy_UpgradePoint>().OpenWindow();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BuyPoint"))
            other.GetComponent<BuyPoint>().StopSpend();

        if (other.gameObject.CompareTag("HelperSpawner"))
        {
            other.GetComponent<HelperBuy_UpgradePoint>().CloseWindow();
        }
    }

    public void IncreasePlayerCapacity()
    {
        if (_GameManager.collectedMoney >= playerCapacityBuyAmount)
        {
            AudioManager.Instance.Play("Upgrade");

            _PlayerManager.maxFoodPlayerCarry++;
            PlayerPrefs.SetInt("PlayerCapacity", _PlayerManager.maxFoodPlayerCarry);

            playerCapacityBuyAmount += 100;
            PlayerPrefs.SetInt("PlayerCapacityBuyAmount", playerCapacityBuyAmount);

            playerCapaciyTest.text = playerCapacityBuyAmount.ToString();
        }
    }
}
