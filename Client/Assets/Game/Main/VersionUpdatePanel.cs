using System;
using Main;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VersionUpdatePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sliderText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private GameObject loadingObj;
    [SerializeField] private GameObject sliderObj;

    public Action DownLoadAction;

    public void Show()
    {
        gameObject.SetActive(true);
        closeBtn.onClick.AddListener(() =>
        {
            
        });
        confirmBtn.onClick.AddListener(() =>
        {
            DownLoadAction?.Invoke();
            ShowProgress();
        });
    }

    public void ShowProgress()
    {
        progressBar.value = 0f;
        sliderObj.SetActive(true);
        loadingObj.SetActive(true);
        sliderText.text = "0%";
        closeBtn.gameObject.SetActive(false);
        confirmBtn.gameObject.SetActive(false);
    }
    
    public void UpdateProgress(float value)
    {
        sliderText.text = $"{(value * 100):F1}%";
        progressBar.value = value;
    }
}
