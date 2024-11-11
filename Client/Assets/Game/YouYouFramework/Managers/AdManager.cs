using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class AdManager : MonoBehaviour
{
    private int sdkInitializedState = -1;//0--unconsent 1--consen

    private string ADMobRewardUnit = "ca-app-pub-3940256099942544/5224354917";
    private string ADMobInterstitialUnit = "ca-app-pub-3940256099942544/1033173712";
    private string ADMobBannerUnit = "ca-app-pub-3940256099942544/6300978111";
	private RewardedAd _rewardedAd = null;
	private InterstitialAd _interstitialAd = null;
    private BannerView _bannerView;

    private int tryInteTimes = 0;
    private int loadInteTimes = 1;

    private int tryTimes = 0;
    private int maxTryTimes = 0;
    private int loadTimes = 1;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
            sdkInitializedState = 1;
            PrepareRewardAds();
            PrepareInterAds();
        });
    }

    private void PrepareRewardAds()
    {
        if (sdkInitializedState < 0)
            return;
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        var adRequest = new AdRequest();
        RewardedAd.Load(ADMobRewardUnit, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

                _rewardedAd = ad;
            });
    }


    private void PrepareInterAds()
    {
        if (sdkInitializedState < 0)
            return;
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(ADMobInterstitialUnit, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());

                _interstitialAd = ad;
            });
    }


    [ContextMenu("测试Banner")]
    public void LoadBannerAd()
    {
        // create an instance of a banner view first.
        if (_bannerView == null)
        {
            CreateBannerView();
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        _bannerView.LoadAd(adRequest);
    }


    /// <summary>
    /// Creates a 320x50 banner view at top of the screen.
    /// </summary>
    private void CreateBannerView()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (_bannerView != null)
        {
            _bannerView.Destroy();
        }

        // Create a 320x50 banner at top of the screen
        _bannerView = new BannerView(ADMobBannerUnit, AdSize.Banner, AdPosition.Bottom);
    }


    [ContextMenu("测试插屏广告")]
    public void ShowInterAD()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            // SetAdmobInterstitialListener(_interstitialAd);
            _interstitialAd.Show();
        }
        else
        {
            if (++this.tryInteTimes >= this.maxTryTimes)
            {
                this.loadInteTimes = 3;
                this.PrepareInterAds();
                this.tryInteTimes = 0;
            }
            return;
        }
    }


    public void ShowRewardAD(Action successCallback)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            SetAdmobRewardListener(_rewardedAd);
            _rewardedAd.Show((Reward reward) =>
            {
                successCallback();
            });
        }
    }


    private void SetAdmobRewardListener(RewardedAd ad)
    {
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            //RewardedAdClicked();
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            PrepareRewardAds();
            //RewardedAdClosed();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content with error : " + error);
            // RewardedAdFailed();
            PrepareRewardAds();
        };
    }

    private void SetAdmobInterstitialListener(InterstitialAd interstitialAd)
    {
        // Raised when a click is recorded for an ad.
        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
            //InterstitialAdClicked();
        };
        // Raised when an ad opened full screen content.
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
           // InterstitialAdDisplayed();
        };
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            //InterstitialAdClosed();
            PrepareInterAds();
        };
        // Raised when the ad failed to open full screen content.
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content with error : " + error);
            //InterstitialAdFailed();
            PrepareInterAds();
        };
    }

    [ContextMenu("测试激励广告")]
    public void TestShowRewardAd()
    {
        ShowRewardAD(() => 
        {
            Debug.LogError("激励广告回调");
        });
    }


}

