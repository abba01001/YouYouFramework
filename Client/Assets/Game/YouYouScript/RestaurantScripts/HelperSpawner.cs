using RDG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class HelperSpawner : MonoBehaviour
{
    public Button helperCapacityBtn, helperBuybtn, helperSpeedBtn;
    public Text capacityBuyValText, speedBuyValText, helperBuyValText;
    public int moneyIncreaseVal, increaseCapacityVal, increaseSpeedVal;
    private int capacityBuyVal, speedBuyValue, helperBuyValue;
    private GameManager _GameManager;
    public GameObject capacityFullText, speedFullText;
    public Helper helper;
    public GameObject helperPrefab;
    public Transform helperSpawnPoint;
    public int srNo;

    private void OnEnable()
    {
        _GameManager = FindObjectOfType<GameManager>();
        UpdateBuyAmountsText();
        CheckButtonsActive();
    }

    public void BuyHelper()
    {
        MyAdManager.Instance.ShowInterstitialAd();

        AudioManager.Instance.Play("Upgrade");
        _GameManager.LessMoneyinBulk(helperBuyValue);

        helper = Instantiate(helperPrefab, helperSpawnPoint.position, helperSpawnPoint.rotation).GetComponent<Helper>();

        PlayerPrefs.SetString(srNo + "Helper", "");

        helperBuybtn.transform.parent.gameObject.SetActive(false);

        helperCapacityBtn.transform.parent.gameObject.SetActive(true);
        helperSpeedBtn.transform.parent.gameObject.SetActive(true);
    }

    public void BuyCapacity()
    {
        MyAdManager.Instance.ShowInterstitialAd();

        AudioManager.Instance.Play("Upgrade");

        _GameManager.LessMoneyinBulk(capacityBuyVal);

        capacityBuyVal += moneyIncreaseVal;
        PlayerPrefs.SetInt(srNo + "CapacityBuyVal", capacityBuyVal);

        UpdateBuyAmountsText();

        if (helper._PlayerManager.maxFoodPlayerCarry == 12)
        {
            capacityFullText.SetActive(true);
            PlayerPrefs.SetString(srNo + "CapacityFull", "True");
        }

        helper.IncreaseCapacity(increaseCapacityVal);

        CheckButtonsActive();
        helper.upgradeParticle.Play();
    }

    public void BuySpeed()
    {
        MyAdManager.Instance.ShowInterstitialAd();

        AudioManager.Instance.Play("Upgrade");

        _GameManager.LessMoneyinBulk(speedBuyValue);

        speedBuyValue += moneyIncreaseVal;
        PlayerPrefs.SetInt(srNo + "SpeedBuyVal", speedBuyValue);

        UpdateBuyAmountsText();

        if (helper.gameObject.GetComponent<NavMeshAgent>().speed == 20)
        {
            speedFullText.SetActive(true);
            PlayerPrefs.SetString(srNo + "SpeedFull", "True");
        }
        helper.IncreaseSpeed(increaseSpeedVal);

        CheckButtonsActive();
        helper.upgradeParticle.Play();

    }

    private void UpdateBuyAmountsText()
    {
        if (PlayerPrefs.HasKey(srNo + "CapacityFull"))
        {
            capacityFullText.SetActive(true);
            helperCapacityBtn.interactable = false;
        }

        if (PlayerPrefs.HasKey(srNo + "SpeedFull"))
        {
            speedFullText.SetActive(true);
            helperSpeedBtn.interactable = false;
        }

        helperBuyValue = moneyIncreaseVal;
        helperBuyValText.text = moneyIncreaseVal.ToString();

        capacityBuyVal = PlayerPrefs.GetInt(srNo + "CapacityBuyVal", moneyIncreaseVal);
        capacityBuyValText.text = capacityBuyVal.ToString();

        speedBuyValue = PlayerPrefs.GetInt(srNo + "SpeedBuyVal", moneyIncreaseVal);
        speedBuyValText.text = speedBuyValue.ToString();
    }

    private void CheckButtonsActive()
    {
        if (PlayerPrefs.HasKey(srNo + "Helper"))
            helperBuybtn.transform.parent.gameObject.SetActive(false);
        else
        {
            if (helperBuyValue <= _GameManager.collectedMoney)
                helperBuybtn.interactable = true;
            else
                helperBuybtn.interactable = false;

            helperCapacityBtn.transform.parent.gameObject.SetActive(false);
            helperSpeedBtn.transform.parent.gameObject.SetActive(false);
        }

        if (capacityBuyVal <= _GameManager.collectedMoney)
            helperCapacityBtn.interactable = true;
        else
            helperCapacityBtn.interactable = false;


        if (speedBuyValue <= _GameManager.collectedMoney)
            helperSpeedBtn.interactable = true;
        else
            helperSpeedBtn.interactable = false;
    }
}
