using System;
using UnityEngine;

namespace PH.Core.Items
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

        public ItemRunEvent(ItemDefinition definition, int absoluteFloor, int pageIndex, int pageFloorIndex, int columnIndex, float runTimeSeconds)
        {
            eventId = Guid.NewGuid().ToString("N");
            itemId = definition.ItemId;
            serverItemId = definition.ServerItemId;
            tableVersion = definition.TableVersion;
            this.absoluteFloor = absoluteFloor;
            this.pageIndex = pageIndex;
            this.pageFloorIndex = pageFloorIndex;
            this.columnIndex = columnIndex;
            effectValue = definition.EffectValue;
            acquiredAtMilliseconds = Mathf.Max(0, Mathf.RoundToInt(runTimeSeconds * 1000f));
        }
    }
}
