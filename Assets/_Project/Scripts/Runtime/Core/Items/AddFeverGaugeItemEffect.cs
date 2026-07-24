using LootUp.Core.Characters;
using UnityEngine;

namespace LootUp.Core.Items
{
    public sealed class AddFeverGaugeItemEffect : IItemEffect
    {
        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null
                && (definition.ItemType == ItemType.Fever || definition.EffectKey == ItemEffectKeys.AddFeverGauge);
        }

        public ItemEffectResult Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null || context.PlayerCharacterRuntime == null)
            {
                context.TopHUDController?.SetItemStatus(definition != null ? definition.DisplayName : "FEVER BATTERY");
                return ItemEffectResult.None;
            }

            PlayerCharacterRuntime runtime = context.PlayerCharacterRuntime;
            int feverGaugeAmount = ResolveFeverGaugeAmount(definition.EffectValue, context.RequiredPassCount);
            float addedAmount = runtime.AddFeverGaugeFromItem(feverGaugeAmount);
            if (addedAmount <= 0f)
            {
                context.TopHUDController?.SetItemStatus(runtime.IsFeverActive ? "FEVER ACTIVE" : "FEVER FULL");
                return new ItemEffectResult(ItemEffectOutcome.FeverGaugeUnavailable, 0);
            }

            int displayedAmount = Mathf.RoundToInt(addedAmount);
            context.TopHUDController?.SetItemStatus($"+{displayedAmount} FEVER");
            return new ItemEffectResult(ItemEffectOutcome.FeverGaugeAdded, displayedAmount);
        }

        private static int ResolveFeverGaugeAmount(int baseAmount, int requiredPassCount)
        {
            int clampedBaseAmount = Mathf.Max(0, baseAmount);
            if (requiredPassCount >= 5)
            {
                return clampedBaseAmount * 6;
            }

            if (requiredPassCount >= 3)
            {
                return clampedBaseAmount * 3;
            }

            return clampedBaseAmount;
        }
    }
}
