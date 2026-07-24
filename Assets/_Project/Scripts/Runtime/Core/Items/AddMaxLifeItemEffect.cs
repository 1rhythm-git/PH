using LootUp.Core.Player;

namespace LootUp.Core.Items
{
    public sealed class AddMaxLifeItemEffect : IItemEffect
    {
        private const int MaxLifeBonusLimit = 1;

        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null && definition.EffectKey == ItemEffectKeys.AddMaxLife;
        }

        public ItemEffectResult Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null || context.TopHUDController == null)
            {
                return ItemEffectResult.None;
            }

            int scoreBonus = definition.EffectValue * context.TopHUDController.FullHeartScoreBonusPerHeart;
            if (context.PlayerHealth == null)
            {
                context.TopHUDController.AddScore(scoreBonus);
                context.TopHUDController.SetItemStatus($"+{scoreBonus} SCORE");
                return new ItemEffectResult(ItemEffectOutcome.ScoreAdded, scoreBonus);
            }

            PlayerMaxLifeItemResult result = context.PlayerHealth.ApplyMaxLifeItem(definition.EffectValue, MaxLifeBonusLimit);
            switch (result)
            {
                case PlayerMaxLifeItemResult.IncreasedMaxLife:
                    context.TopHUDController.SetItemStatus($"+{definition.EffectValue} MAX LIFE");
                    return new ItemEffectResult(ItemEffectOutcome.MaxLifeIncreased, definition.EffectValue);
                case PlayerMaxLifeItemResult.Healed:
                    context.TopHUDController.SetItemStatus($"+{definition.EffectValue} HP");
                    return new ItemEffectResult(ItemEffectOutcome.LifeHealed, definition.EffectValue);
                case PlayerMaxLifeItemResult.ScoreBonus:
                    context.TopHUDController.AddScore(scoreBonus);
                    context.TopHUDController.SetItemStatus($"+{scoreBonus} SCORE");
                    return new ItemEffectResult(ItemEffectOutcome.ScoreAdded, scoreBonus);
                default:
                    context.TopHUDController.SetItemStatus(definition.DisplayName);
                    return ItemEffectResult.None;
            }
        }
    }
}
