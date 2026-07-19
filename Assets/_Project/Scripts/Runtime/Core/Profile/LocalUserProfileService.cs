using System;
using System.Collections.Generic;
using UnityEngine;

namespace PH.Core.Profile
{
    public sealed class LocalUserProfileService : IUserProfileService
    {
        private const string SaveKey = "PH.UserProfile.v1";
        private const string DefaultUserIdPrefix = "guest-";
        private const string DefaultNickname = "Player";

        private readonly UserProfileSaveData saveData;

        public LocalUserProfileService()
        {
            saveData = Load();
            EnsureDefaults();
        }

        public string UserId => saveData.UserId;
        public string Nickname => string.IsNullOrWhiteSpace(saveData.Nickname) ? DefaultNickname : saveData.Nickname;

        public int GetCurrencyAmount(UserCurrencyType currencyType)
        {
            UserCurrencyData entry = FindCurrency(currencyType);
            return entry != null ? Mathf.Max(0, entry.Amount) : 0;
        }

        public IReadOnlyList<UserCurrencyData> GetCurrencies()
        {
            return saveData.Currencies;
        }

        public IReadOnlyList<UserTraitData> GetTraits()
        {
            return saveData.Traits;
        }

        public float GetTraitBonusPercent(UserTraitEffectType effectType)
        {
            if (effectType == UserTraitEffectType.None)
            {
                return 0f;
            }

            float totalValue = 0f;
            for (int i = 0; i < saveData.Traits.Count; i++)
            {
                UserTraitData trait = saveData.Traits[i];
                if (trait != null && trait.EffectType == effectType && trait.Level > 0)
                {
                    totalValue += trait.Value;
                }
            }

            return Mathf.Max(0f, totalValue);
        }

        public void SetIdentity(string userId, string nickname)
        {
            saveData.UserId = string.IsNullOrWhiteSpace(userId) ? saveData.UserId : userId.Trim();
            saveData.Nickname = string.IsNullOrWhiteSpace(nickname) ? DefaultNickname : nickname.Trim();
            TrySave();
        }

        public UserCurrencyChangeResult AddCurrency(UserCurrencyType currencyType, int amount)
        {
            int addAmount = Mathf.Max(0, amount);
            UserCurrencyData entry = FindOrCreateCurrency(currencyType);
            int previousAmount = Mathf.Max(0, entry.Amount);
            entry.Amount = previousAmount + addAmount;
            TrySave();
            return new UserCurrencyChangeResult(addAmount > 0, currencyType, previousAmount, entry.Amount);
        }

        public UserCurrencyChangeResult TrySpendCurrency(UserCurrencyType currencyType, int amount)
        {
            int spendAmount = Mathf.Max(0, amount);
            UserCurrencyData entry = FindOrCreateCurrency(currencyType);
            int previousAmount = Mathf.Max(0, entry.Amount);
            if (spendAmount <= 0 || previousAmount < spendAmount)
            {
                return new UserCurrencyChangeResult(false, currencyType, previousAmount, previousAmount);
            }

            entry.Amount = previousAmount - spendAmount;
            TrySave();
            return new UserCurrencyChangeResult(true, currencyType, previousAmount, entry.Amount);
        }

        public void SetTrait(string traitId, UserTraitEffectType effectType, int level, float value)
        {
            if (string.IsNullOrWhiteSpace(traitId))
            {
                return;
            }

            UserTraitData trait = FindTrait(traitId);
            if (trait == null)
            {
                trait = new UserTraitData { TraitId = traitId.Trim() };
                saveData.Traits.Add(trait);
            }

            trait.EffectType = effectType;
            trait.Level = Mathf.Max(0, level);
            trait.Value = Mathf.Max(0f, value);
            TrySave();
        }

        public bool TrySave()
        {
            try
            {
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"User profile save failed: {exception.Message}");
                return false;
            }
        }

        private UserProfileSaveData Load()
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new UserProfileSaveData();
            }

            try
            {
                UserProfileSaveData loaded = JsonUtility.FromJson<UserProfileSaveData>(json) ?? new UserProfileSaveData();
                loaded.Currencies ??= new List<UserCurrencyData>();
                loaded.Traits ??= new List<UserTraitData>();
                return loaded;
            }
            catch (Exception exception)
            {
                Debug.LogError($"User profile load failed: {exception.Message}");
                return new UserProfileSaveData();
            }
        }

        private void EnsureDefaults()
        {
            if (string.IsNullOrWhiteSpace(saveData.UserId))
            {
                saveData.UserId = DefaultUserIdPrefix + Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrWhiteSpace(saveData.Nickname))
            {
                saveData.Nickname = DefaultNickname;
            }

            FindOrCreateCurrency(UserCurrencyType.GameMoney);
            FindOrCreateCurrency(UserCurrencyType.Ruby);
            TrySave();
        }

        private UserCurrencyData FindOrCreateCurrency(UserCurrencyType currencyType)
        {
            UserCurrencyData entry = FindCurrency(currencyType);
            if (entry != null)
            {
                entry.Amount = Mathf.Max(0, entry.Amount);
                return entry;
            }

            entry = new UserCurrencyData { CurrencyType = currencyType, Amount = 0 };
            saveData.Currencies.Add(entry);
            return entry;
        }

        private UserCurrencyData FindCurrency(UserCurrencyType currencyType)
        {
            for (int i = 0; i < saveData.Currencies.Count; i++)
            {
                UserCurrencyData entry = saveData.Currencies[i];
                if (entry != null && entry.CurrencyType == currencyType)
                {
                    return entry;
                }
            }

            return null;
        }

        private UserTraitData FindTrait(string traitId)
        {
            for (int i = 0; i < saveData.Traits.Count; i++)
            {
                UserTraitData trait = saveData.Traits[i];
                if (trait != null && string.Equals(trait.TraitId, traitId, StringComparison.Ordinal))
                {
                    return trait;
                }
            }

            return null;
        }
    }
}
