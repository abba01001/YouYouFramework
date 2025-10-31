using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YouYou;


// "主"界面
public class FormUpgrade : UIFormBase
{
    protected override void Awake()
    {
        base.Awake();
        panel.MSetActive(false);
    }

    protected override void OnShow()
    {
        base.OnShow();
        panel.MSetActive(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateBuyAmountsText();
        CheckButtonsActive();
        GameEntry.Event.AddEventListener(Constants.EventName.SetMoneyText, SetMoneyText);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEntry.Event.RemoveEventListener(Constants.EventName.SetMoneyText, SetMoneyText);
    }

    public void SetMoneyText(object userdata)
    {
        // collectedMoney.text = "$" + ((int)userdata).ToString();
    }


    public Button helperCapacityBtn, helperBuybtn, helperSpeedBtn;
    public Text capacityBuyValText, speedBuyValText, helperBuyValText;
    public int moneyIncreaseVal, increaseCapacityVal, increaseSpeedVal;
    private int capacityBuyVal, speedBuyValue, helperBuyValue;
    public GameObject capacityFullText, speedFullText;
    public Helper helper;
    public GameObject helperPrefab;
    public Transform helperSpawnPoint;
    public int srNo;
    public GameObject panel;

    public void BuyHelper()
    {
        MyAdManager.Instance.ShowInterstitialAd();

        AudioManager.Instance.Play("Upgrade");
        GameEntry.Data.LessMoney(helperBuyValue);

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

        GameEntry.Data.LessMoney(capacityBuyVal);

        capacityBuyVal += moneyIncreaseVal;
        PlayerPrefs.SetInt(srNo + "CapacityBuyVal", capacityBuyVal);

        UpdateBuyAmountsText();

        // if (helper._PlayerManager.maxFoodPlayerCarry == 12)
        // {
            // capacityFullText.SetActive(true);
            // PlayerPrefs.SetString(srNo + "CapacityFull", "True");
        // }

        helper.IncreaseCapacity(increaseCapacityVal);

        CheckButtonsActive();
        helper.upgradeParticle.Play();
    }

    public void BuySpeed()
    {
        MyAdManager.Instance.ShowInterstitialAd();

        AudioManager.Instance.Play("Upgrade");

        GameEntry.Data.LessMoney(speedBuyValue);

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
            if (helperBuyValue <= GameEntry.Data.Coin)
                helperBuybtn.interactable = true;
            else
                helperBuybtn.interactable = false;

            helperCapacityBtn.transform.parent.gameObject.SetActive(false);
            helperSpeedBtn.transform.parent.gameObject.SetActive(false);
        }

        if (capacityBuyVal <= GameEntry.Data.Coin)
            helperCapacityBtn.interactable = true;
        else
            helperCapacityBtn.interactable = false;


        if (speedBuyValue <= GameEntry.Data.Coin)
            helperSpeedBtn.interactable = true;
        else
            helperSpeedBtn.interactable = false;
    }
}