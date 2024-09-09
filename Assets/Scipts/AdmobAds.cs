using UnityEngine.Events;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AdmobAds : MonoBehaviour
{
    public static AdmobAds instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    BannerView _bannerView;
    [HideInInspector] public RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;
    [HideInInspector] public bool isRewarded, giveReward;

    // IMPORTANT: *** CHANGE THESE VARIABLE VALUES TO YOUR ADMOB ACCOUNT ADUNIT ID ***
    [Header("Android Ad Unit IDs")]
    public string AndroidRewardedAdUnitId;
    public string AndroidBannerAdUnitId;
    public string AndroidInterstitialAdUnitId;

    [Header("iOS Ad Unit IDs")]
    public string IOSRewardedAdUnitId;
    public string IOSBannerAdUnitId;
    public string IOSInterstitialAdUnitId;

    private string RewardedAdUnitId;
    private string BannerAdUnitId;
    private string InterstitialAdUnitId;

    void Start()
    {
#if UNITY_ANDROID
        RewardedAdUnitId = AndroidRewardedAdUnitId;
        BannerAdUnitId = AndroidBannerAdUnitId;
        InterstitialAdUnitId = AndroidInterstitialAdUnitId;
#elif UNITY_IOS
        RewardedAdUnitId = IOSRewardedAdUnitId;
        BannerAdUnitId = IOSBannerAdUnitId;
        InterstitialAdUnitId = IOSInterstitialAdUnitId;
#endif

        MobileAds.Initialize((InitializationStatus initStatus) => { });

        RequestRewardedAd();
        LoadInterstitialAd();
    }

    public void LoadInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");
        var adRequest = new AdRequest.Builder()
            .AddKeyword("unity-admob-sample")
            .Build();

        InterstitialAd.Load(InterstitialAdUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial ad failed to load an ad with error: " + error);
                    return;
                }
                RegisterReloadHandler(ad);
                Debug.Log("Interstitial ad loaded with response: " + ad.GetResponseInfo());
                interstitialAd = ad;
            });
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            interstitialAd.Show();
        }
    }

    private void RegisterReloadHandler(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial Ad full screen content closed.");
            interstitialAd.Destroy();
            LoadInterstitialAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content with error: " + error);
            LoadInterstitialAd();
        };
    }

    public void CreateBannerView()
    {
        Debug.Log("Creating banner view");

        if (_bannerView != null)
        {
            DestroyBannerAd();
        }

        _bannerView = new BannerView(BannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
    }

    public void LoadBannerAd()
    {
        if (_bannerView == null)
        {
            CreateBannerView();
        }

        var adRequest = new AdRequest.Builder()
            .AddKeyword("unity-admob-sample")
            .Build();

        Debug.Log("Loading banner ad.");
        _bannerView.LoadAd(adRequest);
    }

    public void DestroyBannerAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Destroying banner ad.");
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    private void RequestRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var adRequest = new AdRequest.Builder().Build();

        RewardedAd.Load(RewardedAdUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad with error: " + error);
                    return;
                }
                RegisterReloadHandler(ad);
                Debug.Log("Rewarded ad loaded with response: " + ad.GetResponseInfo());
                rewardedAd = ad;
            });
    }

    private void RegisterReloadHandler(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Ad full screen content closed.");
            RequestRewardedAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content with error: " + error);
            RequestRewardedAd();
        };
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    MainMenuHandler.Instance.SelectedCustomization();
                }
            });
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            if (giveReward)
                isRewarded = true;
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
        };

        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };

        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            isRewarded = false;
            giveReward = false;
            Debug.Log("Rewarded ad full screen content closed.");
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content with error: " + error);
        };
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args) { }

    public void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) { }

    public void HandleRewardedAdOpening(object sender, EventArgs args) { }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args) { }

    private void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        isRewarded = false;
        giveReward = false;
    }

    private void HandleUserEarnedReward(object sender, Reward args)
    {
        if (giveReward)
            isRewarded = true;
    }
}
