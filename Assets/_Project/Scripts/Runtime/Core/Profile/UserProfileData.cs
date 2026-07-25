using System;
using System.Collections.Generic;

namespace LootUp.Core.Profile
{
    public enum UserCurrencyType
    {
        GameMoney,
        Ruby
    }

    public enum UserTraitEffectType
    {
        None,
        CollectionItemChanceBonusPercent,
        ArtifactChanceBonusPercent,
        CharacterCoinChanceBonusPercent
    }

    [Serializable]
    public sealed class UserCurrencyData
    {
        public UserCurrencyType CurrencyType;
        public int Amount;
    }

    [Serializable]
    public sealed class UserTraitData
    {
        public string TraitId;
        public UserTraitEffectType EffectType;
        public int Level;
        public float Value;
    }

    [Serializable]
    public sealed class UserProfileSaveData
    {
        public int Version = 2;
        public string UserId;
        public string Nickname = "Player";
        public int BestHighestFloor;
        public int BestScore;
        public string BestCharacterId;
        public List<UserCurrencyData> Currencies = new List<UserCurrencyData>();
        public List<UserTraitData> Traits = new List<UserTraitData>();
    }

    public readonly struct UserCurrencyChangeResult
    {
        public UserCurrencyChangeResult(bool applied, UserCurrencyType currencyType, int previousAmount, int currentAmount)
        {
            Applied = applied;
            CurrencyType = currencyType;
            PreviousAmount = Math.Max(0, previousAmount);
            CurrentAmount = Math.Max(0, currentAmount);
        }

        public bool Applied { get; }
        public UserCurrencyType CurrencyType { get; }
        public int PreviousAmount { get; }
        public int CurrentAmount { get; }
        public int DeltaAmount => CurrentAmount - PreviousAmount;
    }
}
