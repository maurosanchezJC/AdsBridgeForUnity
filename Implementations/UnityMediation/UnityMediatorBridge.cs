#if UNITY_MEDIATION_PACKAGE_IMPLEMENTED
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Mediation;
using UnityEngine;
using Action = System.Action;

namespace JCUnityTeam.AdsImplementation.UnityMediation
{
    public class UnityMediatorBridge : IAdsMediator
    {
        private const int MAX_RETRIES = 3;
        private string gameId = null;

        private AdsMediationFactory<RewardedAdWrapper, RewardedAdWrapper> rewardedAdsFactory;
        private AdsMediationFactory<InterstitialAdWrapper, InterstitialAdWrapper> interstitialAdsFactory;

        private AdsMediationServiceConfig Config { get; }

        public string AdProvider { get; } = "UnityMediation";
        public bool IsInitialized => UnityServices.State == ServicesInitializationState.Initialized;

        public UnityMediatorBridge(AdsMediationServiceConfig config)
        {
            Config = config;
            gameId = config.GetGameIDForPlatform(Application.platform);
        }
        

        public async void InitMediator(Action onInitCompleted)
        {
            InitializationOptions options = GetInitAdsOptions();
            Task task = UnityServices.InitializeAsync(options);

            if (IsInitialized) //This can be simplified. There's a reading error.
            {
                await task;
                task.Dispose();
                InitAdsFactory();
                onInitCompleted?.Invoke();
            }

            //retry
            int currentCount = 0;
            while (currentCount < MAX_RETRIES && !IsInitialized)
            {
                try
                {
                    await task;
                    task.Dispose();
                    InitAdsFactory();
                    onInitCompleted?.Invoke();
                }
                catch (Exception e)
                {
                    task.Dispose();
                    currentCount++;

                    Console.WriteLine(e);
                    await Task.Delay(1000);
                }
            }
            
        }
        
        private InitializationOptions GetInitAdsOptions()
        {
            InitializationOptions options = new InitializationOptions();
            options.SetGameId(gameId);
            return options;
        }

        private void InitAdsFactory()
        {
            rewardedAdsFactory = new AdsMediationFactory<RewardedAdWrapper, RewardedAdWrapper>(Config.RewardedAdsData);
            interstitialAdsFactory = new AdsMediationFactory<InterstitialAdWrapper, InterstitialAdWrapper>(Config.InterstitialAdsData);
        }

        public void SubscribeListener(IAdListener listener)
        {
            rewardedAdsFactory.SubscribeListener(listener);
            interstitialAdsFactory.SubscribeListener(listener);
        }
        public void UnsubscribeListener(IAdListener listener)
        {
            rewardedAdsFactory.UnsubscribeListener(listener);
            interstitialAdsFactory.UnsubscribeListener(listener);
        }

        public void ShowAd(AdPlacement placement, AdType adType)
        {
            switch (adType)
            {
                case AdType.Rewarded:
                    rewardedAdsFactory.ShowAd(placement);
                    return;
                case AdType.Interstitial:
                    interstitialAdsFactory.ShowAd(placement);
                    return;
            }
        }
        
        public bool IsAdLoaded(AdPlacement placement, AdType adType)
        {
            switch (adType)
            {
                case AdType.Rewarded:
                    return rewardedAdsFactory.IsAdLoaded(placement);
                case AdType.Interstitial:
                    return interstitialAdsFactory.IsAdLoaded(placement);
                default:
                    return false;
            }
        }

        public void ReloadAllAds()
        {
            rewardedAdsFactory.ReloadAllAds();
            interstitialAdsFactory.ReloadAllAds();
        }
    }
}

#endif