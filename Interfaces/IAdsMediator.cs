using Action = System.Action;

namespace JCUnityTeam.AdsImplementation
{
    public interface IAdsMediator
    {
        string AdProvider { get; }
        bool IsInitialized { get; }
        void InitMediator(Action onInitCompleted);
        void SubscribeListener(IAdListener listener);
        void UnsubscribeListener(IAdListener listener);

        void ShowAd(AdPlacement placement, AdType adType);
        bool IsAdLoaded(AdPlacement placement, AdType adType);
        void ReloadAllAds();
    }
}