using UnityEngine;

namespace LootUp.Core.Items
{
    public sealed class AddScoreItemEffect : IItemEffect
    {
        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null && (definition.ItemType == ItemType.Score || definition.EffectKey == ItemEffectKeys.AddScore);
        }

        public ItemEffectResult Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null || context.TopHUDController == null)
            {
                return ItemEffectResult.None;
            }

            int bonusPercent = Mathf.Max(0, context.ScoreBonusPercent);
            int scoreAmount = Mathf.Max(0, Mathf.RoundToInt(definition.EffectValue * (100f + bonusPercent) / 100f));
            if (ArtifactEffectResolver.RollPercent(ArtifactEffectResolver.Resolve().ScoreItemDoubleChancePercent))
            {
                scoreAmount *= 2;
            }

            context.TopHUDController.AddScore(scoreAmount);
            context.TopHUDController.SetItemStatus($"+{scoreAmount} SCORE");
            return new ItemEffectResult(ItemEffectOutcome.ScoreAdded, scoreAmount);
        }
    }
}
