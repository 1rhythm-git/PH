using System;
using UnityEngine;

namespace LootUp.Core.Items
{
    [Serializable]
    public sealed class ItemRunEvent
    {
        [SerializeField]
        private string eventId;

        [SerializeField]
        private string itemId;

        [SerializeField]
        private string serverItemId;

        [SerializeField]
        private string tableVersion;

        [SerializeField]
        private int absoluteFloor;

        [SerializeField]
        private int pageIndex;

        [SerializeField]
        private int pageFloorIndex;

        [SerializeField]
        private int columnIndex;

        [SerializeField]
        private int effectValue;

        [SerializeField]
        private int acquiredAtMilliseconds;

        [SerializeField]
        private ItemEffectOutcome effectOutcome;

        [SerializeField]
        private int appliedValue;

        [SerializeField]
        private CollectionChangeStatus collectionStatus;

        [SerializeField]
        private bool applied;

        public string EventId => eventId;
        public string ItemId => itemId;
        public string ServerItemId => serverItemId;
        public string TableVersion => tableVersion;
        public int AbsoluteFloor => absoluteFloor;
        public int PageIndex => pageIndex;
        public int PageFloorIndex => pageFloorIndex;
        public int ColumnIndex => columnIndex;
        public int EffectValue => effectValue;
        public int AcquiredAtMilliseconds => acquiredAtMilliseconds;
        public ItemEffectOutcome EffectOutcome => effectOutcome;
        public int AppliedValue => appliedValue;
        public CollectionChangeStatus CollectionStatus => collectionStatus;
        public bool Applied => applied;

        public ItemRunEvent(ItemDefinition definition, int absoluteFloor, int pageIndex, int pageFloorIndex, int columnIndex, float runTimeSeconds)
            : this(Guid.NewGuid().ToString("N"), definition, absoluteFloor, pageIndex, pageFloorIndex, columnIndex, runTimeSeconds, ItemEffectResult.None)
        {
        }

        public ItemRunEvent(string acquisitionEventId, ItemDefinition definition, int absoluteFloor, int pageIndex, int pageFloorIndex, int columnIndex, float runTimeSeconds, ItemEffectResult effectResult)
        {
            eventId = string.IsNullOrWhiteSpace(acquisitionEventId) ? Guid.NewGuid().ToString("N") : acquisitionEventId;
            itemId = definition.ItemId;
            serverItemId = definition.ServerItemId;
            tableVersion = definition.TableVersion;
            this.absoluteFloor = absoluteFloor;
            this.pageIndex = pageIndex;
            this.pageFloorIndex = pageFloorIndex;
            this.columnIndex = columnIndex;
            effectValue = definition.EffectValue;
            acquiredAtMilliseconds = Mathf.Max(0, Mathf.RoundToInt(runTimeSeconds * 1000f));
            effectOutcome = effectResult.Outcome;
            appliedValue = effectResult.Value;
            collectionStatus = effectResult.CollectionStatus;
            applied = definition.ItemType != ItemType.Collection || effectResult.Outcome == ItemEffectOutcome.CollectionAdded;
        }
    }
}
