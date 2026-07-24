using System;
using System.Collections.Generic;
using LootUp.Core.Characters;
using UnityEngine;

namespace LootUp.Core.Items
{
    public sealed class LocalCollectionInventoryService : ICollectionInventoryService
    {
        private const int CurrentVersion = 2;
        private const string SaveKey = "LootUp.CollectionProgress.v1";
        private const string LegacySaveKey = "PH.CollectionProgress.v1";

        private readonly CollectionSaveData saveData;

        public LocalCollectionInventoryService()
        {
            saveData = Load();
            NormalizeSaveData();
        }

        public int GetOwnedAmount(string collectionId)
        {
            CollectionData entry = FindCollection(collectionId);
            return entry != null ? Mathf.Max(0, entry.OwnedAmount) : 0;
        }

        public int GetLifetimeAcquiredAmount(string collectionId)
        {
            CollectionData entry = FindCollection(collectionId);
            return entry != null ? Mathf.Max(0, entry.LifetimeAcquiredAmount) : 0;
        }

        public int GetCharacterUpgradeLevel(string characterId, string upgradeId)
        {
            CharacterUpgradeData entry = FindUpgrade(CharacterIdMigration.Normalize(characterId), NormalizeKey(upgradeId));
            return entry != null ? Mathf.Max(0, entry.Level) : 0;
        }

        public IReadOnlyList<CollectionData> GetCollections()
        {
            return saveData.Collections;
        }

        public IReadOnlyList<PendingCollectionEventData> GetPendingEvents()
        {
            return saveData.PendingEvents;
        }

        public CollectionChangeResult TryAcquire(ItemDefinition definition, string eventId)
        {
            if (definition == null || definition.ItemType != ItemType.Collection || string.IsNullOrWhiteSpace(definition.CollectionId))
            {
                return new CollectionChangeResult(CollectionChangeStatus.InvalidDefinition, eventId, string.Empty, 0, 0);
            }

            if (HasPendingEvent(eventId))
            {
                int ownedAmount = GetOwnedAmount(definition.CollectionId);
                return new CollectionChangeResult(CollectionChangeStatus.DuplicateEvent, eventId, definition.CollectionId, ownedAmount, ownedAmount);
            }

            CollectionData entry = FindCollection(definition.CollectionId);
            int previousAmount = entry != null ? Mathf.Max(0, entry.OwnedAmount) : 0;
            int maxOwnedAmount = Mathf.Max(0, definition.MaxOwnedAmount);

            if (definition.CollectionItemType == CollectionItemType.Artifact && previousAmount > 0)
            {
                return new CollectionChangeResult(CollectionChangeStatus.AlreadyOwned, eventId, definition.CollectionId, previousAmount, previousAmount);
            }

            if (maxOwnedAmount > 0 && previousAmount >= maxOwnedAmount)
            {
                return new CollectionChangeResult(CollectionChangeStatus.OwnedLimitReached, eventId, definition.CollectionId, previousAmount, previousAmount);
            }

            int acquireAmount = Mathf.Max(1, definition.AcquireAmount);
            int currentAmount = maxOwnedAmount > 0
                ? Mathf.Min(maxOwnedAmount, previousAmount + acquireAmount)
                : previousAmount + acquireAmount;
            int addedAmount = currentAmount - previousAmount;
            if (addedAmount <= 0)
            {
                return new CollectionChangeResult(CollectionChangeStatus.OwnedLimitReached, eventId, definition.CollectionId, previousAmount, previousAmount);
            }

            if (entry == null)
            {
                entry = new CollectionData
                {
                    CollectionId = definition.CollectionId,
                    CollectionItemType = definition.CollectionItemType
                };
                saveData.Collections.Add(entry);
            }

            entry.CollectionItemType = definition.CollectionItemType;

            entry.OwnedAmount = currentAmount;
            entry.LifetimeAcquiredAmount = Mathf.Max(0, entry.LifetimeAcquiredAmount) + addedAmount;
            saveData.PendingEvents.Add(CreatePendingEvent(definition, eventId, addedAmount));

            CollectionChangeStatus status = TrySave() ? CollectionChangeStatus.Added : CollectionChangeStatus.SavePending;
            return new CollectionChangeResult(status, eventId, definition.CollectionId, previousAmount, currentAmount);
        }

        public CharacterUpgradeResult TryApplyCharacterUpgrade(string characterId, string upgradeId, IReadOnlyList<CollectionCost> costs)
        {
            characterId = CharacterIdMigration.Normalize(characterId);
            upgradeId = NormalizeKey(upgradeId);
            CharacterUpgradeData upgrade = FindUpgrade(characterId, upgradeId);
            int previousLevel = upgrade != null ? Mathf.Max(0, upgrade.Level) : 0;
            Dictionary<string, int> aggregatedCosts = AggregateCosts(costs);
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(upgradeId) || aggregatedCosts.Count <= 0)
            {
                return new CharacterUpgradeResult(CharacterUpgradeStatus.InvalidRequest, previousLevel, previousLevel);
            }

            foreach (KeyValuePair<string, int> cost in aggregatedCosts)
            {
                CollectionData collection = FindCollection(cost.Key);
                if (collection == null
                    || collection.CollectionItemType != CollectionItemType.CharacterCoin
                    || collection.OwnedAmount < cost.Value)
                {
                    return new CharacterUpgradeResult(CharacterUpgradeStatus.InsufficientCoins, previousLevel, previousLevel);
                }
            }

            foreach (KeyValuePair<string, int> cost in aggregatedCosts)
            {
                CollectionData collection = FindCollection(cost.Key);
                collection.OwnedAmount = Mathf.Max(0, collection.OwnedAmount - cost.Value);
            }

            if (upgrade == null)
            {
                upgrade = new CharacterUpgradeData { CharacterId = characterId, UpgradeId = upgradeId };
                saveData.CharacterUpgrades.Add(upgrade);
            }

            upgrade.Level = previousLevel + 1;
            CharacterUpgradeStatus status = TrySave() ? CharacterUpgradeStatus.Applied : CharacterUpgradeStatus.SavePending;
            return new CharacterUpgradeResult(status, previousLevel, upgrade.Level);
        }

        public bool TrySave()
        {
            try
            {
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
                PlayerPrefs.DeleteKey(LegacySaveKey);
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Collection save failed: {exception.Message}");
                return false;
            }
        }

        private CollectionSaveData Load()
        {
            string json = GetSavedJson();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new CollectionSaveData();
            }

            try
            {
                CollectionSaveData loaded = JsonUtility.FromJson<CollectionSaveData>(json) ?? new CollectionSaveData();
                loaded.Collections ??= new List<CollectionData>();
                loaded.CharacterUpgrades ??= new List<CharacterUpgradeData>();
                loaded.PendingEvents ??= new List<PendingCollectionEventData>();
                return loaded;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Collection load failed: {exception.Message}");
                return new CollectionSaveData();
            }
        }

        private static string GetSavedJson()
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            return string.IsNullOrWhiteSpace(json)
                ? PlayerPrefs.GetString(LegacySaveKey, string.Empty)
                : json;
        }

        private CollectionData FindCollection(string collectionId)
        {
            if (string.IsNullOrWhiteSpace(collectionId))
            {
                return null;
            }

            for (int i = 0; i < saveData.Collections.Count; i++)
            {
                CollectionData entry = saveData.Collections[i];
                if (entry != null && string.Equals(entry.CollectionId, collectionId, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        private void NormalizeSaveData()
        {
            saveData.Version = CurrentVersion;
            Dictionary<(string CharacterId, string UpgradeId), CharacterUpgradeData> knownUpgrades =
                new Dictionary<(string CharacterId, string UpgradeId), CharacterUpgradeData>();
            for (int i = saveData.CharacterUpgrades.Count - 1; i >= 0; i--)
            {
                CharacterUpgradeData entry = saveData.CharacterUpgrades[i];
                if (entry == null)
                {
                    saveData.CharacterUpgrades.RemoveAt(i);
                    continue;
                }

                entry.CharacterId = CharacterIdMigration.Normalize(entry.CharacterId);
                entry.UpgradeId = NormalizeKey(entry.UpgradeId);
                entry.Level = Mathf.Max(0, entry.Level);
                if (string.IsNullOrEmpty(entry.CharacterId) || string.IsNullOrEmpty(entry.UpgradeId))
                {
                    saveData.CharacterUpgrades.RemoveAt(i);
                    continue;
                }

                (string CharacterId, string UpgradeId) key = (entry.CharacterId, entry.UpgradeId);
                if (knownUpgrades.TryGetValue(key, out CharacterUpgradeData existingEntry))
                {
                    existingEntry.Level = Mathf.Max(existingEntry.Level, entry.Level);
                    saveData.CharacterUpgrades.RemoveAt(i);
                    continue;
                }

                knownUpgrades.Add(key, entry);
            }

            TrySave();
        }

        private CharacterUpgradeData FindUpgrade(string characterId, string upgradeId)
        {
            for (int i = 0; i < saveData.CharacterUpgrades.Count; i++)
            {
                CharacterUpgradeData entry = saveData.CharacterUpgrades[i];
                if (entry != null
                    && string.Equals(entry.CharacterId, characterId, StringComparison.Ordinal)
                    && string.Equals(entry.UpgradeId, upgradeId, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        private string NormalizeKey(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private Dictionary<string, int> AggregateCosts(IReadOnlyList<CollectionCost> costs)
        {
            Dictionary<string, int> result = new Dictionary<string, int>(StringComparer.Ordinal);
            if (costs == null)
            {
                return result;
            }

            for (int i = 0; i < costs.Count; i++)
            {
                CollectionCost cost = costs[i];
                if (cost == null || string.IsNullOrWhiteSpace(cost.CollectionId) || cost.Amount <= 0)
                {
                    continue;
                }

                result.TryGetValue(cost.CollectionId, out int currentAmount);
                result[cost.CollectionId] = currentAmount + cost.Amount;
            }

            return result;
        }

        private PendingCollectionEventData CreatePendingEvent(ItemDefinition definition, string eventId, int amount)
        {
            return new PendingCollectionEventData
            {
                EventId = eventId,
                CollectionId = definition.CollectionId,
                ItemId = definition.ItemId,
                ServerItemId = definition.ServerItemId,
                TableVersion = definition.TableVersion,
                Amount = amount,
                AcquiredAtUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }

        private bool HasPendingEvent(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return false;
            }

            for (int i = 0; i < saveData.PendingEvents.Count; i++)
            {
                PendingCollectionEventData pendingEvent = saveData.PendingEvents[i];
                if (pendingEvent != null && string.Equals(pendingEvent.EventId, eventId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
