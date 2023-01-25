using System.Collections.Generic;
using jc.analytics.@event;
using UnityEngine;

namespace JCUnityTeam.AdsImplementation
{
    public class AdsMediationFactory<T, T2> where T : IAdWrapper<T2>, new() where T2 : IAdWrapper, new()
    {
        private Dictionary<AdPlacement, T> adWrappers;

        public AdsMediationFactory(List<AdUnitData> adUnitList)
        {
            CreateAdInstances(adUnitList);
        }

        private void CreateAdInstances(List<AdUnitData> adUnitData)
        {
            adWrappers = new Dictionary<AdPlacement, T>();
            foreach (var adUnit in adUnitData)
            {
                if (adWrappers.ContainsKey(adUnit.placement))
                {
                    Debug.LogError($"There are multiple requests to register the add with placement {adUnit.placement}");
                    continue;
                }
                
                string adUnitId = adUnit.GetAdUnitForPlatform(Application.platform);
                T adWrapper = new T();
                adWrapper.Setup(adUnit.placement, adUnitId, true);
                adWrappers.Add(adUnit.placement, adWrapper);
            }
        }

        private string GetAdUnitID(AdPlacement adPlacement) => adWrappers.ContainsKey(adPlacement) ? adWrappers[adPlacement].AdUnitID : string.Empty;

        public void LoadAd(AdPlacement adPlacement)
        {
            if (!adWrappers.ContainsKey(adPlacement))
            {
                Debug.LogError("The ad doesn't exist");
            }

            adWrappers[adPlacement].LoadAd();
        }

        public void ReloadAllAds()
        {
            foreach (T interstitialAd in adWrappers.Values)
            {
                interstitialAd.LoadAd();
            }
        }

        public void ShowAd(AdPlacement adPlacement)
        {
            if (!adWrappers.ContainsKey(adPlacement))
            {
                Debug.LogError($"The {adPlacement} ad is not possible to show");
                return;
            }

            if (!adWrappers[adPlacement].IsLoaded)
            {
                Debug.Log($"AD STATE: {adWrappers[adPlacement].State}");
            }

            Debug.Log($"Showing {adPlacement} Ad");
            adWrappers[adPlacement].ShowAd();
        }

        public bool IsAdLoaded(AdPlacement adPlacement)
        {
            return adWrappers.ContainsKey(adPlacement) && adWrappers[adPlacement].IsLoaded;
        }

        public void SubscribeListener(IAdListener listener)
        {
            foreach (var adReward in adWrappers.Values)
            { 
                adReward.SubscribeListener(listener);
            }
        }
        public void UnsubscribeListener(IAdListener listener)
        {
            foreach (var adReward in adWrappers.Values)
            { 
                adReward.UnsubscribeListener(listener);
            }
        }
    }
}
