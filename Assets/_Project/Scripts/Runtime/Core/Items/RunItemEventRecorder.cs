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

        [ContextMenu("Debug/Clear Item Events")]
        private void DebugClearItemEvents()
        {
            acquiredItemEvents.Clear();
        }
    }
}
