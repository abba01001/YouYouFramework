using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasUiManager : MonoBehaviour
{
    public Text collectedMoney;
    public GameObject dragToMoveWindow;
    public GameObject settingsPanel;

    private void Update()
    {
        if (Input.GetMouseButton(0) && dragToMoveWindow)
        {
            PlayerPrefs.SetString("DragWindow","");
            Destroy(dragToMoveWindow);
        }
    }

    private void Start()
    {
        if(PlayerPrefs.HasKey("DragWindow"))
            Destroy(dragToMoveWindow);
        Application.targetFrameRate = 60; 
    }

    public void SetMoneyText(int amount)
    {
        collectedMoney.text = "$" + amount.ToString();
    }

    public void Reload()
    {
        MyAdManager.Instance.ShowInterstitialAd();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GetRewardCash()
    {
        MyAdManager.Instance.ShowRewardVideo();
    }

    public void OpenSettingsWindow()
    {
        MyAdManager.Instance.ShowInterstitialAd();
        settingsPanel.SetActive(true);
    }
}
