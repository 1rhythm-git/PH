using System;
using System.Collections.Generic;

namespace LootUp.Core.Items
{
    public enum CollectionItemType
    {
        None,
        Artifact,
        CharacterCoin
    }

    public enum CollectionChangeStatus
    {
        Added,
        AlreadyOwned,
        OwnedLimitReached,
        RunLimitReached,
        DuplicateEvent,
        InvalidDefinition,
        SavePending
    }

    public enum CharacterUpgradeStatus
    {
        Applied,
        InvalidRequest,
        InsufficientCoins,
        SavePending
    }

    [Serializable]
    public sealed class CollectionData
    {
        public string CollectionId;
        public CollectionItemType CollectionItemType;
        public int OwnedAmount;
        public int LifetimeAcquiredAmount;
    }

    [Serializable]
    public sealed class CharacterUpgradeData
    {
        public string CharacterId;
        public string UpgradeId;
        public int Level;
    }

    [Serializable]
    public sealed class CollectionCost
    {
        public string CollectionId;
        public int Amount;

        public CollectionCost(string collectionId, int amount)
        {
            CollectionId = collectionId;
            Amount = Math.Max(0, amount);
        }
    }

    [Serializable]
    public sealed class PendingCollectionEventData
    {
        public string EventId;
        public string CollectionId;
        public string ItemId;
        public string ServerItemId;
        public string TableVersion;
        public int Amount;
        public long AcquiredAtUnixMilliseconds;
    }

    [Serializable]
    public sealed class CollectionSaveData
    {
        public int Version = 2;
        public List<CollectionData> Collections = new List<CollectionData>();
        public List<CharacterUpgradeData> CharacterUpgrades = new List<CharacterUpgradeData>();
        public List<PendingCollectionEventData> PendingEvents = new List<PendingCollectionEventData>();
    }

    public readonly struct CollectionChangeResult
    {
        public CollectionChangeResult(CollectionChangeStatus status, string eventId, string collectionId, int previousAmount, int currentAmount)
        {
            Status = status;
            EventId = eventId ?? string.Empty;
            CollectionId = collectionId ?? string.Empty;
            PreviousAmount = Math.Max(0, previousAmount);
            CurrentAmount = Math.Max(0, currentAmount);
        }

        public CollectionChangeStatus Status { get; }
        public string EventId { get; }
        public string CollectionId { get; }
        public int PreviousAmount { get; }
        public int CurrentAmount { get; }
        public int AddedAmount => Math.Max(0, CurrentAmount - PreviousAmount);
        public bool Applied => Status == CollectionChangeStatus.Added || Status == CollectionChangeStatus.SavePending;
    }

    public readonly struct CharacterUpgradeResult
    {
        public CharacterUpgradeResult(CharacterUpgradeStatus status, int previousLevel, int currentLevel)
        {
            Status = status;
            PreviousLevel = Math.Max(0, previousLevel);
            CurrentLevel = Math.Max(0, currentLevel);
        }

        public CharacterUpgradeStatus Status { get; }
        public int PreviousLevel { get; }
        public int CurrentLevel { get; }
        public bool Applied => Status == CharacterUpgradeStatus.Applied || Status == CharacterUpgradeStatus.SavePending;
    }
}
