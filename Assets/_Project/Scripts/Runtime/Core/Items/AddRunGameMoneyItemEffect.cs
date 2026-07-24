using UnityEngine;

namespace LootUp.Core.Items
{
    public sealed class AddRunGameMoneyItemEffect : IItemEffect
    {
        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null
                && (definition.ItemType == ItemType.Currency || definition.EffectKey == ItemEffectKeys.AddRunGameMoney);
        }

        public ItemEffectResult Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null || context.TopHUDController == null)
            {
                return ItemEffectResult.None;
            }

            int amount = Mathf.Max(0, definition.EffectValue);
            context.TopHUDController.AddRunGameMoney(amount);
            context.TopHUDController.SetItemStatus($"+{amount} MONEY");
            return new ItemEffectResult(ItemEffectOutcome.RunGameMoneyAdded, amount);
        }
    }
}
