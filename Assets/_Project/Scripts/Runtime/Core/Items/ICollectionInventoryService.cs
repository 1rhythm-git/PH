using System.Collections.Generic;

namespace PH.Core.Items
{
    public interface ICollectionInventoryService
    {
        int GetOwnedAmount(string collectionId);
        int GetLifetimeAcquiredAmount(string collectionId);
        int GetCharacterUpgradeLevel(string characterId, string upgradeId);
        IReadOnlyList<CollectionData> GetCollections();
        IReadOnlyList<PendingCollectionEventData> GetPendingEvents();
        CollectionChangeResult TryAcquire(ItemDefinition definition, string eventId);
        CharacterUpgradeResult TryApplyCharacterUpgrade(string characterId, string upgradeId, IReadOnlyList<CollectionCost> costs);
        bool TrySave();
    }
}
