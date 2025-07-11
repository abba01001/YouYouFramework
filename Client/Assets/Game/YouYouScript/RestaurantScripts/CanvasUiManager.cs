using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YouYou;

public class CanvasUiManager : MonoBehaviour
{
    public Text collectedMoney;
    public GameObject dragToMoveWindow;
    public GameObject settingsPanel;
    public GameObject player;
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
        transform.AddComponent<MeshCollider>();
        Vector3 orignPos = player.transform.localPosition;
        player.transform.localPosition = orignPos + new Vector3(0, 10, 0);
        if(PlayerPrefs.HasKey("DragWindow"))
            Destroy(dragToMoveWindow);

        GameEntry.Time.CreateTimer(this, 2f, () =>
        {
            player.MSetActive(true);
        });
    }

    public void SetMoneyText(int amount)
    {
        collectedMoney.text = "$" + amount.ToString();
    }

    public void Reload()
    {
        MyAdManager.Instance.ShowInterstitialAd();
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GetRewardCash()
    {
        MyAdManager.Instance.ShowRewardVideo();
    }

    public void OpenSettingsWindow()
    {
        PlayerPrefs.DeleteAll();
        MyAdManager.Instance.ShowInterstitialAd();
        settingsPanel.SetActive(true);
    }
}
