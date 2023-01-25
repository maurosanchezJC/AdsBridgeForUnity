using System;
using jc.analytics.@event;

namespace JCUnityTeam.AdsImplementation
{
    /// <summary>
    /// Contains standard callbacks used y Ad Mediators in order to notify other objects about the Ads Status.
    /// Suggested to use with <see cref="SubsriberHandler"/> to gather and notify all listeners.
    /// </summary>
    public interface IAdListener
    {
        /// <summary>
        /// Notifies where the Ad was loaded successfully.
        /// </summary>
        void OnAdLoaded(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs);
        
        /// <summary>
        /// Notifies where the Ad couldn't be loaded.
        /// </summary>
        void OnAdFailedToLoad(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs);
        
        /// <summary>
        /// Notifies where the Ad was showed.
        /// </summary>
        void OnAdShowed(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs);
        
        /// <summary>
        /// Notifies where the Ad failed to Show.
        /// </summary>
        void OnAdFailedToShow(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs);
        
        /// <summary>
        /// Notifies where the Ad was closed.
        /// </summary>
        void OnAdClosed(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs);
        
        /// <summary>
        /// Notifies where the Ad was clicked. (Before Show Events)
        /// </summary>
        void OnAdClicked(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs);
        
        void OnAdUserRewarded(AdPlacement placement, AdType adType, object obj, EventArgs eventArgs);
    }
}