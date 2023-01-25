#if UNITY_MEDIATION_PACKAGE_IMPLEMENTED
using System;
using jc.analytics.@event;
using Unity.Services.Mediation;
using UnityEngine;

namespace JCUnityTeam.AdsImplementation.UnityMediation
{
    public class InterstitialAdWrapper : IAdWrapper<InterstitialAdWrapper>, IDisposable
    {
        private IInterstitialAd interstitialAd;
        public string AdUnitID { get; set; }
        public AdPlacement Placement { get; private set; }
        public AdType AdType => AdType.Interstitial;
        public AdState State => interstitialAd.AdState;
        public bool IsLoaded => interstitialAd.AdState == AdState.Loaded;
        
        private SubscriberHandler<IAdListener> subscriptionHandler = new SubscriberHandler<IAdListener>();
        
        public void Setup(AdPlacement placement, string adUnitId, bool loadOnCreation)
        {
            Placement = placement;
            AdUnitID = adUnitId;
            interstitialAd = MediationService.Instance.CreateInterstitialAd(adUnitId);
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
                interstitialAd.LoadAsync();
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
                interstitialAd.ShowAsync();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void SubscribeListener(IAdListener listener) => subscriptionHandler.Subscribe(listener);

        public void UnsubscribeListener(IAdListener listener) => subscriptionHandler.Unsubscribe(listener);
        
        private void Bind()
        {
            interstitialAd.OnLoaded += OnAdLoaded;
            interstitialAd.OnFailedLoad += OnAdFailedToLoad;
            interstitialAd.OnShowed += OnAdShowed;
            interstitialAd.OnFailedShow += OnAdFailedToShow;
            interstitialAd.OnClicked += OnAdClicked;
            interstitialAd.OnClosed += OnAdClosed;
        }
        private void Unbind()
        {
            interstitialAd.OnLoaded -= OnAdLoaded;
            interstitialAd.OnFailedLoad -= OnAdFailedToLoad;
            interstitialAd.OnShowed -= OnAdShowed;
            interstitialAd.OnFailedShow -= OnAdFailedToShow;
            interstitialAd.OnClicked -= OnAdClicked;
            interstitialAd.OnClosed -= OnAdClosed;
        }

        private void OnAdLoaded(object obj, EventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdLoaded(Placement, AdType, obj, eventArgs));
        }

        private void OnAdFailedToLoad(object obj, LoadErrorEventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdFailedToLoad(Placement, AdType, obj, eventArgs));
        }

        private void OnAdShowed(object obj, EventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdShowed(Placement, AdType, obj, eventArgs));
            LoadAd();
        }

        private void OnAdFailedToShow(object obj, ShowErrorEventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdFailedToShow(Placement, AdType, obj, eventArgs));
            LoadAd();
        }

        private void OnAdClosed(object obj, EventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdClosed(Placement, AdType, obj, eventArgs));
            LoadAd();
        }

        private void OnAdClicked(object obj, EventArgs eventArgs)
        {
            subscriptionHandler.Call(x => x.OnAdClicked(Placement, AdType, obj, eventArgs));
        }

        public void Dispose()
        {
            Unbind();
            interstitialAd?.Dispose();
        }
    }
}
#endif