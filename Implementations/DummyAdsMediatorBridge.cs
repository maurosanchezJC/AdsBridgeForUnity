using jc.analytics.@event;
using UnityEngine;
using Action = System.Action;

namespace JCUnityTeam.AdsImplementation
{
    public class DummyAdsMediatorBridge : IAdsMediator
    {
        private SubscriberHandler<IAdListener> listeners = new SubscriberHandler<IAdListener>();

        public DummyAdsMediatorBridge(AdsMediationServiceConfig config)
        {
        }
            
        public string AdProvider => "DummyMediator";
        public bool IsInitialized { get; private set; }
        public void InitMediator(Action onInitCompleted)
        {
            IsInitialized = true;
            onInitCompleted?.Invoke();
        }

        public void SubscribeListener(IAdListener listener) => listeners.Subscribe(listener);

        public void UnsubscribeListener(IAdListener listener) => listeners.Unsubscribe(listener);

        public void ShowAd(AdPlacement placement, AdType adType)
        {
            Debug.Log($"{GetType()} :: Show Ad {placement} of type {adType} and returned true :)");
            listeners.Call(x => x.OnAdShowed(placement, adType, null, null));
        }

        public bool IsAdLoaded(AdPlacement placement, AdType adType)
        {
            return true;
        }

        public void ReloadAllAds() { }
    }
}
