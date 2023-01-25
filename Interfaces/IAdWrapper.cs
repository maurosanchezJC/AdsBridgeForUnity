using jc.analytics.@event;

namespace JCUnityTeam.AdsImplementation
{
    public interface IAdWrapper
    {
        string AdUnitID { get; set; }
        AdPlacement Placement { get; }
        AdType AdType { get; }
        AdState State { get; }
        bool IsLoaded { get; }
        void Setup(AdPlacement placement, string adUnitId, bool loadOnCreation);
        void LoadAd();
        void ShowAd();
        void Dispose();
    }

    public interface IAdWrapper<T> : IAdWrapper where T : IAdWrapper
    {
        void SubscribeListener(IAdListener listener);
        void UnsubscribeListener(IAdListener listener);
    }
}
