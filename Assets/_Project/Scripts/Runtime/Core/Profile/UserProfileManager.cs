using System;

namespace LootUp.Core.Profile
{
    public static class UserProfileManager
    {
        private static IUserProfileService service;

        public static event Action ProfileChanged;

        public static IUserProfileService Service => service ??= new LocalUserProfileService();
        public static string UserId => Service.UserId;
        public static string Nickname => Service.Nickname;
        public static int BestHighestFloor => Service.BestHighestFloor;
        public static int BestScore => Service.BestScore;
        public static string BestCharacterId => Service.BestCharacterId;
        public static int BestCharacterLevel => Service.BestCharacterLevel;
        public static int GameMoney => Service.GetCurrencyAmount(UserCurrencyType.GameMoney);
        public static int Ruby => Service.GetCurrencyAmount(UserCurrencyType.Ruby);

        public static void Configure(IUserProfileService profileService)
        {
            service = profileService ?? new LocalUserProfileService();
            ProfileChanged?.Invoke();
        }

        public static void SetIdentity(string userId, string nickname)
        {
            Service.SetIdentity(userId, nickname);
            ProfileChanged?.Invoke();
        }

        public static bool TrySetBestRun(
            int highestFloor,
            int score,
            string characterId,
            int characterLevel)
        {
            bool updated =
                Service.TrySetBestRun(
                    highestFloor,
                    score,
                    characterId,
                    characterLevel);
            if (updated)
            {
                ProfileChanged?.Invoke();
            }

            return updated;
        }

        public static UserCurrencyChangeResult AddCurrency(UserCurrencyType currencyType, int amount)
        {
            UserCurrencyChangeResult result = Service.AddCurrency(currencyType, amount);
            if (result.Applied)
            {
                ProfileChanged?.Invoke();
            }

            return result;
        }

        public static void ApplyAuthoritativeCurrencyBalances(
            int gameMoney,
            int ruby)
        {
            Service.SetCurrencyAmount(
                UserCurrencyType.GameMoney,
                gameMoney);
            Service.SetCurrencyAmount(
                UserCurrencyType.Ruby,
                ruby);
            ProfileChanged?.Invoke();
        }

        public static UserCurrencyChangeResult TrySpendCurrency(UserCurrencyType currencyType, int amount)
        {
            UserCurrencyChangeResult result = Service.TrySpendCurrency(currencyType, amount);
            if (result.Applied)
            {
                ProfileChanged?.Invoke();
            }

            return result;
        }

        public static void SetTrait(string traitId, UserTraitEffectType effectType, int level, float value)
        {
            Service.SetTrait(traitId, effectType, level, value);
            ProfileChanged?.Invoke();
        }

        public static float GetCollectionTraitChanceBonusPercent()
        {
            return Service.GetTraitBonusPercent(UserTraitEffectType.CollectionItemChanceBonusPercent);
        }

        public static float GetArtifactChanceBonusPercent()
        {
            return Service.GetTraitBonusPercent(UserTraitEffectType.ArtifactChanceBonusPercent);
        }

        public static float GetCharacterCoinChanceBonusPercent()
        {
            return Service.GetTraitBonusPercent(UserTraitEffectType.CharacterCoinChanceBonusPercent);
        }
    }
}
