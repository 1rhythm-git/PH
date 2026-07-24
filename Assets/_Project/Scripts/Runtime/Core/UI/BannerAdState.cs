using System;
using UnityEngine;

namespace LootUp.Core.UI
{
    public static class BannerAdState
    {
        private const string AdsRemovedSaveKey = "LootUp.Advertising.AdsRemoved.v1";
        private const string DefaultBannerLabel = "AD";

        public static event Action<bool> AdsRemovedChanged;

        public static bool AdsRemoved => PlayerPrefs.GetInt(AdsRemovedSaveKey, 0) == 1;
        public static bool IsBannerVisible => !AdsRemoved;
        public static string BannerLabel => DefaultBannerLabel;

        public static void SetAdsRemoved(bool adsRemoved)
        {
            // 실제 구매 연동 시 영수증 또는 서버 검증이 끝난 뒤 이 진입점을 호출한다.
            if (AdsRemoved == adsRemoved)
            {
                return;
            }

            PlayerPrefs.SetInt(AdsRemovedSaveKey, adsRemoved ? 1 : 0);
            PlayerPrefs.Save();
            AdsRemovedChanged?.Invoke(adsRemoved);
        }
    }
}
