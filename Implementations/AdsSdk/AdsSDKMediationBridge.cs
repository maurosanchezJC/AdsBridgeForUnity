// NOTE: This Bridge depends on the instalation of Ads SDK and IronSource Bridge SDK.
// These two packags can be implemented through JC Package Manager within all their dependencies.

#if JC_ADSSDK_V1 && JC_IRONSOURCEBRIDGESDK_V1
using System;
using System.Collections.Generic;
using JamCity.AdsSdk;
using JamCity.AdsSdk.Config;
using JamCity.AdsSdk.Optimization;
using JamCity.AdsSdk.Settings;
using JamCity.AdsSdk.Unity;
using JamCity.CommonSdk.Executor;
using JamCity.IronSourceBridgeSdk;
using UnityEngine;
using Action = System.Action;
using JCAdType = JamCity.AdsSdk.Config.AdType;

namespace JCUnityTeam.AdsImplementation.AdsSDK
{
    public class AdsSDKMediationBridge : IAdsMediator, IAdListener
    {
        private IAdService adsService;
        private IronSourceBridgeService adMediator;
        private AdsMediationServiceConfig config;
        private Action onInitCompleted;

        public AdsSDKMediationBridge(AdsMediationServiceConfig config)
        {
            this.config = config;
        }

        private SubscriberHandler<IAdListener> subscriptions = new SubscriberHandler<IAdListener>();

        public string AdProvider => "IronSource";
        public bool IsInitialized { get; private set; }
        
        public void InitMediator(Action initCallback)
        {
            onInitCompleted = initCallback;
            adMediator = CreateMediator();
            adMediator.OnMediatorInitialized += OnMediatorInitialized;
            AdConfig adConfig = CreateAdsConfig(config);
            AdSettings adSettings = CreateAdSettings(config);
            UnityAdService.Init(adSettings, adConfig, adMediator);
            
            adsService = UnityAdService.Service;
        }

        private void OnMediatorInitialized()
        {
            adMediator.OnMediatorInitialized += OnMediatorInitialized;
            ReloadAllAds();
            onInitCompleted?.Invoke();
            onInitCompleted = null;
            IsInitialized = true;
        }
        
        private IronSourceBridgeService CreateMediator()
        {
            IronSourceBridgeService.Init();
            IronSourceBridgeService service = (IronSourceBridgeService)IronSourceBridgeService.Service;
            return service;
        }

        private AdSettings CreateAdSettings(AdsMediationServiceConfig gameConfig)
        {
            string gameId = gameConfig.GetGameIDForPlatform(Application.platform);
            return AdSettingsBuilder.Create().
                MediationAppId(gameId).
                EnableAutoReloading(true).
                Build;
        }

        private AdConfig CreateAdsConfig(AdsMediationServiceConfig gameConfig)
        {
            List<Placement> adsPlacements = ConvertAdUnitsToPlacement(gameConfig.RewardedAdsData, JCAdType.RewardedVideo);
            adsPlacements.AddRange(ConvertAdUnitsToPlacement(gameConfig.InterstitialAdsData, JCAdType.Interstitial));
            
            List<Trigger> adTriggers = ConvertAdUnitsToTriggers(gameConfig.RewardedAdsData);
            adTriggers.AddRange(ConvertAdUnitsToTriggers(gameConfig.InterstitialAdsData));

            return new AdConfig()
            {
                Placements = adsPlacements,
                Triggers = adTriggers
            };
        }

        private List<Placement> ConvertAdUnitsToPlacement(List<AdUnitData> adUnits, JCAdType type)
        {
            List<Placement> adsPlacements = new List<Placement>(adUnits.Count);
            foreach (AdUnitData adUnit in adUnits)
            {
                PlacementMetadata metadata = new PlacementMetadata(type, adUnit.placement.ToString(), 15, 0.01f);
                Placement adPlacement = new Placement(adUnit.GetAdUnitForPlatform(Application.platform), metadata);
                
                adsPlacements.Add(adPlacement);
            }

            return adsPlacements;
        }
        private List<Trigger> ConvertAdUnitsToTriggers(List<AdUnitData> adUnits)
        {
            List<Trigger> adTriggers = new List<Trigger>(adUnits.Count);
            foreach (AdUnitData adUnit in adUnits)
            {
                Trigger newTrigger = new Trigger()
                {
                    Name = adUnit.placement.ToString(),
                    IsAvailable = true,
                    RewardAdDailyCap = 999,
                    AdSearchOptions = new AdSearchOptions(targetPlacementName: adUnit.placement.ToString())
                };
                
                adTriggers.Add(newTrigger);
            }

            return adTriggers;
        }

        public void SubscribeListener(IAdListener listener) => subscriptions.Subscribe(listener);

        public void UnsubscribeListener(IAdListener listener) => subscriptions.Unsubscribe(listener);

        public void ShowAd(AdPlacement placement, AdType adType)
        {
            IFuture<AdEventResult> future = adsService.ShowAdForTrigger(placement.ToString(), ConvertAdTypeToAdsSDKType(adType));

            OnAdClicked(placement, adType, null, null);
            future.Then(result => ProcessShowAdResult(result, placement, adType ));
        }

        private void ProcessShowAdResult(AdEventResult adResult, AdPlacement placement, AdType adType)
        {            
            if (!adResult.Succeed)
            {
                OnAdFailedToShow(placement, adType, adResult, null);
                return;
            }

            if (adType == AdType.Rewarded && adResult.Rewarded)
            {
                OnAdUserRewarded(placement, adType, adResult, null);
            }

            //These events happens consecutively due to the future callback.
            OnAdShowed(placement, adType, adResult, null);
            OnAdClosed(placement, adType, adResult, null);
        }
        
        private void LoadAdForPlacement(AdPlacement placement, AdType type)
        {
            JCAdType adType = ConvertAdTypeToAdsSDKType(type);

            IFuture<AdEventResult> future = adsService.LoadAdForTrigger(placement.ToString(), adType);
            future.Then(result => ProcessAdLoadResult(result, placement, type));
        }
        
        private void ProcessAdLoadResult(AdEventResult adResult, AdPlacement placement, AdType adType)
        {
            if (adResult.Succeed)
            {
                OnAdLoaded(placement, adType, adResult, null);
            }
            else
            {
                OnAdFailedToLoad(placement, adType, adResult, null);
            }
        }

        public bool IsAdLoaded(AdPlacement placement, AdType adType)
        {
            JCAdType convertedAdType = ConvertAdTypeToAdsSDKType(adType);
                
            var future = adsService.IsAdAvailableForTrigger(placement.ToString(), convertedAdType);
            return future.IsDone && future.Get().Succeed;
        }

        private JCAdType ConvertAdTypeToAdsSDKType(AdType adType)
        {
            switch (adType)
            {
                case AdType.Rewarded:
                    return JCAdType.RewardedVideo;
                case AdType.Interstitial:
                    return JCAdType.Interstitial;
                case AdType.Banner:
                    return JCAdType.Banner;
                default:
                    throw new Exception("Ad Type cannot be converted to Ads SDK Type!");
            }
        }

        public void ReloadAllAds()
        {
            foreach (AdUnitData rewardedAd in config.RewardedAdsData)
            {
                LoadAdForPlacement(rewardedAd.placement, AdType.Rewarded);
            }
            
            foreach (AdUnitData interstitialAd in config.InterstitialAdsData)
            {
                LoadAdForPlacement(interstitialAd.placement, AdType.Interstitial);
            }
        }
        
#region IAdListener Callback

        public void OnAdLoaded(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs)
        {
            subscriptions.Call(x => x.OnAdLoaded(placement, adType, obj, eventArgs));
        }

        public void OnAdFailedToLoad(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs)
        {
            subscriptions.Call(x => x.OnAdFailedToLoad(placement, adType, obj, eventArgs));
        }

        public void OnAdShowed(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs)
        {
            subscriptions.Call(x => x.OnAdShowed(placement, adType, obj, eventArgs));
        }

        public void OnAdFailedToShow(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs)
        {
            subscriptions.Call(x => x.OnAdFailedToShow(placement, adType, obj, eventArgs));
        }

        public void OnAdClosed(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs)
        {
            subscriptions.Call(x => x.OnAdClosed(placement, adType, obj, eventArgs));
            LoadAdForPlacement(placement, adType);
        }

        public void OnAdClicked(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs)
        {
            subscriptions.Call(x => x.OnAdClicked(placement, adType, obj, eventArgs));
        }

        public void OnAdUserRewarded(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs)
        {
            subscriptions.Call(x => x.OnAdUserRewarded(placement, adType, obj, eventArgs));
        }

#endregion
    }
}
#endif