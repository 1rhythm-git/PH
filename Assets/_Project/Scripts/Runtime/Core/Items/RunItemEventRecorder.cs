using System.Collections.Generic;
using UnityEngine;

namespace PH.Core.Items
{
    public sealed class RunItemEventRecorder : MonoBehaviour
    {
        [SerializeField]
        private List<ItemRunEvent> acquiredItemEvents = new List<ItemRunEvent>();

        public IReadOnlyList<ItemRunEvent> AcquiredItemEvents => acquiredItemEvents;

        public void Record(ItemRunEvent itemEvent)
        {
            if (itemEvent == null)
            {
                return;
            }

            acquiredItemEvents.Add(itemEvent);
            Debug.Log($"Item acquired: {itemEvent.ItemId} floor={itemEvent.AbsoluteFloor} column={itemEvent.ColumnIndex}");
        }

        public int GetAcquireCount(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < acquiredItemEvents.Count; i++)
            {
                ItemRunEvent itemEvent = acquiredItemEvents[i];
                if (itemEvent != null && itemEvent.ItemId == itemId && itemEvent.Applied)
                {
                    count++;
                }
            }

            return count;
        }

        public bool HasReachedAcquireLimit(ItemDefinition definition)
        {
            return definition != null
                && definition.MaxAcquirePerRun > 0
                && GetAcquireCount(definition.ItemId) >= definition.MaxAcquirePerRun;
        }

        [ContextMenu("Debug/Clear Item Events")]
        private void DebugClearItemEvents()
        {
            acquiredItemEvents.Clear();
        }
    }
}
