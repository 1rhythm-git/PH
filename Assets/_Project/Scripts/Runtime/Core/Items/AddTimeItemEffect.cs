using UnityEngine;

namespace PH.Core.Items
{
    public sealed class AddTimeItemEffect : IItemEffect
    {
        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null && (definition.ItemType == ItemType.Time || definition.EffectKey == ItemEffectKeys.AddTime);
        }

        public ItemEffectResult Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null || context.TopHUDController == null)
            {
                return ItemEffectResult.None;
            }

            int passCount = Mathf.Max(1, context.RequiredPassCount);
            int timeAmount = Mathf.Max(0, definition.EffectValue * passCount);
            context.TopHUDController.AddTime(timeAmount);
            context.TopHUDController.SetItemStatus($"+{timeAmount}s TIME");
            return new ItemEffectResult(ItemEffectOutcome.TimeAdded, timeAmount);
        }
    }
}
