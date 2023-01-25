using System;
using System.Collections.Generic;
using jc.analytics.@event;
using UnityEngine;

namespace JCUnityTeam.AdsImplementation
{
    [CreateAssetMenu(fileName = "AdsMediatorConfig", menuName = "Ads Implementation/Ads Mediator Config")]
    public class AdsMediationServiceConfig : ScriptableObject
    {
        public string androidGameID;
        public string iOSGameID;

        [SerializeField] private List<AdUnitData> rewardedAdsData = new List<AdUnitData>();
        [SerializeField] private List<AdUnitData> interstitialAdsData = new List<AdUnitData>();

        public List<AdUnitData> RewardedAdsData => rewardedAdsData;
        public List<AdUnitData> InterstitialAdsData => interstitialAdsData;

        public string GetGameIDForPlatform(RuntimePlatform runtimePlatform)
        {
            switch (runtimePlatform)
            {
                case RuntimePlatform.Android:
                    return androidGameID;
                case RuntimePlatform.IPhonePlayer:
                    return iOSGameID;
                default:
                    return androidGameID;
            }
        }
    }

    [Serializable]
    public class AdUnitData
    {
        public AdPlacement placement;
        public string androidAdUnit;
        public string iosAdUnit;

        public AdPlacement Placement => placement;

        public string GetAdUnitForPlatform(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return androidAdUnit;
                case RuntimePlatform.IPhonePlayer:
                    return iosAdUnit;
                default:
                    return androidAdUnit;
            }
        }
    }
}