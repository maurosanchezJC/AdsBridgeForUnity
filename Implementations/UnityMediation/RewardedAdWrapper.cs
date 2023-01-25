#if UNITY_MEDIATION_PACKAGE_IMPLEMENTED

using System;
using jc.analytics.@event;
using Unity.Services.Mediation;
using UnityEngine;

namespace JCUnityTeam.AdsImplementation.UnityMediation
{
    public class RewardedAdWrapper : IAdWrapper<RewardedAdWrapper>, IDisposable
    {
        private IRewardedAd _rewardedAd;

        private SubscriberHandler<IAdListener> subscriptionHandler = new SubscriberHandler<IAdListener>();

        public string AdUnitID { get; set; }

        public AdPlacement Placement { get; private set; }
        public AdType AdType { get; private set; }
        public AdState State => _rewardedAd.AdState;
        public bool IsLoaded => _rewardedAd.AdState == AdState.Loaded;

        public void Setup(AdPlacement placement, string adUnitId, bool loadOnCreation)
        {
            Placement = placement;
            AdUnitID = adUnitId;
            _rewardedAd = MediationService.Instance.CreateRewardedAd(adUnitId);
            Bind();

            if (loadOnCreation)
            {
                LoadAd();
            }
        }

        public void LoadAd()
        {
            if (IsLoaded) 
                return;
            
            try
            {
                _rewardedAd.LoadAsync();
                Debug.Log($"Loaded {AdUnitID} Ad");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        public void ShowAd()
        {
            try
            {
                _rewardedAd.ShowAsync();
                Debug.Log($"Showing {AdUnitID} Ad");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        public void Dispose()
        {
            Unbind();
            _rewardedAd?.Dispose();
        }
        
        public void SubscribeListener(IAdListener listener) => subscriptionHandler.Subscribe(listener);

        public void UnsubscribeListener(IAdListener listener) => subscriptionHandler.Unsubscribe(listener);

        private void Bind()
        {
            _rewardedAd.OnLoaded += OnAdLoaded;
            _rewardedAd.OnFailedLoad += OnAdFailedToLoad;
            _rewardedAd.OnShowed += OnAdShowed;
            _rewardedAd.OnFailedShow += OnAdFailedToShow;
            _rewardedAd.OnClicked += OnAdClicked;
            _rewardedAd.OnUserRewarded += OnAdUserRewarded;
            _rewardedAd.OnClosed += OnAdClosed;
        }
        private void Unbind()
        {
            _rewardedAd.OnLoaded -= OnAdLoaded;
            _rewardedAd.OnFailedLoad -= OnAdFailedToLoad;
            _rewardedAd.OnShowed -= OnAdShowed;
            _rewardedAd.OnFailedShow -= OnAdFailedToShow;
            _rewardedAd.OnClicked -= OnAdClicked;
            _rewardedAd.OnUserRewarded -= OnAdUserRewarded;
            _rewardedAd.OnClosed -= OnAdClosed;
        }

        private void OnAdLoaded(object obj, EventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdLoaded(Placement, AdType, obj, eventArgs));
            Debug.Log($"Rewarded Ad :: {AdUnitID} loaded.");
        }

        private void OnAdFailedToLoad(object obj, LoadErrorEventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdFailedToLoad(Placement, AdType, obj, eventArgs));
            Debug.Log($"Rewarded Ad :: {AdUnitID} failed to load.");
        }

        private void OnAdShowed(object obj, EventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdShowed(Placement, AdType, obj, eventArgs));
            Debug.Log($"Rewarded Ad :: {AdUnitID} showed.");
            LoadAd();
        }

        private void OnAdFailedToShow(object obj, ShowErrorEventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdFailedToShow(Placement, AdType, obj, eventArgs));
            Debug.Log($"Rewarded Ad :: {AdUnitID} failed to show.");
            LoadAd();
        }

        private void OnAdClosed(object obj, EventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdClosed(Placement, AdType, obj, eventArgs));
            Debug.Log($"Rewarded Ad :: {AdUnitID} closed.");
            LoadAd();
        }

        private void OnAdClicked(object obj, EventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdClicked(Placement, AdType, obj, eventArgs));

            Debug.Log($"Rewarded Ad :: {AdUnitID} clicked.");
        }
        private void OnAdUserRewarded(object obj, RewardEventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdUserRewarded(Placement, AdType, obj, eventArgs));
            Debug.Log($"Rewarded Ad :: {AdUnitID} rewarded.");
        }
    }
}

#endif