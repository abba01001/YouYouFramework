using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class MyAdManager : MonoBehaviour
{
    private string appId = "ca-app-pub-2470247100960862~9764865990";
    private string bannerId = "ca-app-pub-3940256099942544/6300978111";
    private string intertestialId = "ca-app-pub-3940256099942544/1033173712";
    private string rewardId = "ca-app-pub-3940256099942544/5224354917";


    [HideInInspector]
    public static MyAdManager Instance;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {

        // Initialize the Google Mobile Ads SDK.

        this.RequestBanner();

        this.RequestInterstitial();
        RequestRewardBasedVideo();

    }

    private void RequestBanner()
    {
        // Create a 320x50 banner at the top of the screen.
    }

    private void RequestInterstitial()
    {
    }
    public void ShowInterstitialAd()
    {
    }

    public void ShowRewardVideo()
    {
        print("RewardAds");

        RequestRewardBasedVideo();
    }

    private void RequestRewardBasedVideo()
    {
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        RequestRewardBasedVideo();
    }

}



