using System;
using System.Collections.Generic;

namespace PH.Core.Items
{
    public static class ItemCollectionManager
    {
        private static ICollectionInventoryService service;

        public static event Action<CollectionChangeResult> CollectionChanged;
        public static event Action<string, string, CharacterUpgradeResult> CharacterUpgradeChanged;

        public static ICollectionInventoryService Service => service ??= new LocalCollectionInventoryService();

        public static void Configure(ICollectionInventoryService inventoryService)
        {
            service = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        }

        public static int GetOwnedAmount(string collectionId)
        {
            return Service.GetOwnedAmount(collectionId);
        }

        public static bool HasReachedOwnedLimit(ItemDefinition definition)
        {
            if (definition == null || definition.ItemType != ItemType.Collection || string.IsNullOrWhiteSpace(definition.CollectionId))
            {
                return false;
            }

            int ownedAmount = GetOwnedAmount(definition.CollectionId);
            return definition.CollectionItemType == CollectionItemType.Artifact
                ? ownedAmount > 0
                : definition.MaxOwnedAmount > 0 && ownedAmount >= definition.MaxOwnedAmount;
        }

        public static CollectionChangeResult TryAcquire(ItemDefinition definition, string eventId)
        {
            CollectionChangeResult result = Service.TryAcquire(definition, eventId);
            if (result.Applied)
            {
                CollectionChanged?.Invoke(result);
            }

            return result;
        }

        public static CharacterUpgradeResult TryApplyCharacterUpgrade(string characterId, string upgradeId, IReadOnlyList<CollectionCost> costs)
        {
            CharacterUpgradeResult result = Service.TryApplyCharacterUpgrade(characterId, upgradeId, costs);
            if (result.Applied)
            {
                CharacterUpgradeChanged?.Invoke(characterId, upgradeId, result);
            }

            return result;
        }

        public static int GetCharacterUpgradeLevel(string characterId, string upgradeId)
        {
            return Service.GetCharacterUpgradeLevel(characterId, upgradeId);
        }
    }
}
