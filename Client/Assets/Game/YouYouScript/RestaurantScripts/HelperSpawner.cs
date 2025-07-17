using Cysharp.Threading.Tasks;
using RDG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using YouYou;

public class HelperSpawner : MonoBehaviour
{
    public Button helperCapacityBtn, helperBuybtn, helperSpeedBtn;
    public Text capacityBuyValText, speedBuyValText, helperBuyValText;
    public int moneyIncreaseVal, increaseCapacityVal, increaseSpeedVal;
    private int capacityBuyVal, speedBuyValue, helperBuyValue;
    public GameObject capacityFullText, speedFullText;
    public Helper helper;
    public GameObject helperPrefab;
    public Transform helperSpawnPoint;
    public int srNo;

    void Start()
    {
        InitWorker();
    }

    public async UniTask InitWorker()
    {
        PoolObj helper = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/RestaurantPrefab/Helper.prefab");
        helper.gameObject.MSetActive(true);
        helper.transform.position = transform.position;
        helper.transform.rotation = transform.rotation;
        helper.transform.SetParent(this.transform);
        
    }
    //
    // private void OnEnable()
    // {
    //     UpdateBuyAmountsText();
    //     CheckButtonsActive();
    // }
    //
    // public void BuyHelper()
    // {
    //     MyAdManager.Instance.ShowInterstitialAd();
    //
    //     AudioManager.Instance.Play("Upgrade");
    //     GameEntry.Data.LessMoney(helperBuyValue);
    //
    //     helper = Instantiate(helperPrefab, helperSpawnPoint.position, helperSpawnPoint.rotation).GetComponent<Helper>();
    //
    //     PlayerPrefs.SetString(srNo + "Helper", "");
    //
    //     helperBuybtn.transform.parent.gameObject.SetActive(false);
    //
    //     helperCapacityBtn.transform.parent.gameObject.SetActive(true);
    //     helperSpeedBtn.transform.parent.gameObject.SetActive(true);
    // }
    //
    // public void BuyCapacity()
    // {
    //     MyAdManager.Instance.ShowInterstitialAd();
    //
    //     AudioManager.Instance.Play("Upgrade");
    //
    //     GameEntry.Data.LessMoney(capacityBuyVal);
    //
    //     capacityBuyVal += moneyIncreaseVal;
    //     PlayerPrefs.SetInt(srNo + "CapacityBuyVal", capacityBuyVal);
    //
    //     UpdateBuyAmountsText();
    //
    //     if (helper._PlayerManager.maxFoodPlayerCarry == 12)
    //     {
    //         capacityFullText.SetActive(true);
    //         PlayerPrefs.SetString(srNo + "CapacityFull", "True");
    //     }
    //
    //     helper.IncreaseCapacity(increaseCapacityVal);
    //
    //     CheckButtonsActive();
    //     helper.upgradeParticle.Play();
    // }
    //
    // public void BuySpeed()
    // {
    //     MyAdManager.Instance.ShowInterstitialAd();
    //
    //     AudioManager.Instance.Play("Upgrade");
    //
    //     GameEntry.Data.LessMoney(speedBuyValue);
    //
    //     speedBuyValue += moneyIncreaseVal;
    //     PlayerPrefs.SetInt(srNo + "SpeedBuyVal", speedBuyValue);
    //
    //     UpdateBuyAmountsText();
    //
    //     if (helper.gameObject.GetComponent<NavMeshAgent>().speed == 20)
    //     {
    //         speedFullText.SetActive(true);
    //         PlayerPrefs.SetString(srNo + "SpeedFull", "True");
    //     }
    //     helper.IncreaseSpeed(increaseSpeedVal);
    //
    //     CheckButtonsActive();
    //     helper.upgradeParticle.Play();
    //
    // }
    //
    // private void UpdateBuyAmountsText()
    // {
    //     if (PlayerPrefs.HasKey(srNo + "CapacityFull"))
    //     {
    //         capacityFullText.SetActive(true);
    //         helperCapacityBtn.interactable = false;
    //     }
    //
    //     if (PlayerPrefs.HasKey(srNo + "SpeedFull"))
    //     {
    //         speedFullText.SetActive(true);
    //         helperSpeedBtn.interactable = false;
    //     }
    //
    //     helperBuyValue = moneyIncreaseVal;
    //     helperBuyValText.text = moneyIncreaseVal.ToString();
    //
    //     capacityBuyVal = PlayerPrefs.GetInt(srNo + "CapacityBuyVal", moneyIncreaseVal);
    //     capacityBuyValText.text = capacityBuyVal.ToString();
    //
    //     speedBuyValue = PlayerPrefs.GetInt(srNo + "SpeedBuyVal", moneyIncreaseVal);
    //     speedBuyValText.text = speedBuyValue.ToString();
    // }
    //
    // private void CheckButtonsActive()
    // {
    //     if (PlayerPrefs.HasKey(srNo + "Helper"))
    //         helperBuybtn.transform.parent.gameObject.SetActive(false);
    //     else
    //     {
    //         if (helperBuyValue <= GameEntry.Data.Coin)
    //             helperBuybtn.interactable = true;
    //         else
    //             helperBuybtn.interactable = false;
    //
    //         helperCapacityBtn.transform.parent.gameObject.SetActive(false);
    //         helperSpeedBtn.transform.parent.gameObject.SetActive(false);
    //     }
    //
    //     if (capacityBuyVal <= GameEntry.Data.Coin)
    //         helperCapacityBtn.interactable = true;
    //     else
    //         helperCapacityBtn.interactable = false;
    //
    //
    //     if (speedBuyValue <= GameEntry.Data.Coin)
    //         helperSpeedBtn.interactable = true;
    //     else
    //         helperSpeedBtn.interactable = false;
    // }
}
