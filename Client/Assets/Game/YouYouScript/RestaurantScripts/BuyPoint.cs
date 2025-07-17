using UnityEngine;
using RDG;
using TMPro;
using DG.Tweening;
using YouYou;

public class BuyPoint : TriggetBase
{
    public int srNo, purchaseAmount;
    private float countAnimSpeed = 0.1f;
    private float animDuration = 0.5f;
    private TextMeshPro moneyAmountText;
    public GameObject objectToUnlock;

    private void Awake()
    {
        return;
        if (PlayerPrefs.HasKey(srNo + "Unlocked"))
        {
            if (objectToUnlock.GetComponent<FoodPlaceManager>())
            {
                objectToUnlock.GetComponent<BoxCollider>().enabled = true;
            }
            objectToUnlock.SetActive(true);
            Destroy(this.gameObject);
        }
        else
        {
            objectToUnlock.SetActive(false);
        }


        purchaseAmount = PlayerPrefs.GetInt(srNo+"PurchaseAmount", purchaseAmount);

        moneyAmountText = GetComponentInChildren<TextMeshPro>();

        ShowPurchaseAmount();
    }

    public void OnEnable()
    {
        
    }
    public void OnDisable()
    {
        
    }
    
    private void ShowPurchaseAmount()
    {
        moneyAmountText.text = purchaseAmount.ToString();
    }

    public void StartSpend()
    {
        if (purchaseAmount > 100)
            countAnimSpeed = 0.05f;
         else if (purchaseAmount > 500)
            countAnimSpeed = 0.01f;

        InvokeRepeating("Spend", countAnimSpeed, countAnimSpeed);
    }

    private void Spend()
    {
        if (GameEntry.Data.Coin > 0)
        {       
            AudioManager.Instance.Play("BuyPoint");

            Vibration.Vibrate(30);
            purchaseAmount--;
            PlayerPrefs.SetInt(srNo + "PurchaseAmount", purchaseAmount);

            GameEntry.Data.LessMoney();
            ShowPurchaseAmount();

            if (purchaseAmount == 0)
            {
                PlayerPrefs.SetString(srNo + "Unlocked", "True");

                BuildingSystem.Instance.PlayerController.SidePos();
                objectToUnlock.transform.DOPunchScale(new Vector3(0.1f, 1, 0.1f), animDuration, 7).OnComplete(() => Destroy(this.gameObject)); ;
                UnlockObject();
                CustomerSpawner[] custSawners = FindObjectsOfType<CustomerSpawner>();
                custSawners[Random.Range(0, custSawners.Length)].SpawnCustomer();

                AudioManager.Instance.Play("Unlock");
                ParticleSystem particle = GetComponentInChildren<ParticleSystem>();
                particle.transform.parent = null;
                particle.Play();
            }
        }
        else
        {
            CancelInvoke("Spend");
        }
    }
    
    private void UnlockObject()
    {
        objectToUnlock.SetActive(true);
        DOTween.Kill(this.gameObject);      
        Destroy(this.gameObject);
    }

    public void StopSpend()
    {
        CancelInvoke("Spend");
    }
}
