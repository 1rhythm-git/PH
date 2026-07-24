using System.Collections.Generic;

namespace LootUp.Core.Profile
{
    public interface IUserProfileService
    {
        string UserId { get; }
        string Nickname { get; }
        int GetCurrencyAmount(UserCurrencyType currencyType);
        IReadOnlyList<UserCurrencyData> GetCurrencies();
        IReadOnlyList<UserTraitData> GetTraits();
        float GetTraitBonusPercent(UserTraitEffectType effectType);
        void SetIdentity(string userId, string nickname);
        UserCurrencyChangeResult AddCurrency(UserCurrencyType currencyType, int amount);
        UserCurrencyChangeResult TrySpendCurrency(UserCurrencyType currencyType, int amount);
        void SetTrait(string traitId, UserTraitEffectType effectType, int level, float value);
        bool TrySave();
    }
}
