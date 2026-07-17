namespace PH.Core.Items
{
    public sealed class HealHeartItemEffect : IItemEffect
    {
        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null && (definition.ItemType == ItemType.Heal || definition.EffectKey == ItemEffectKeys.HealHeart);
        }

        public ItemEffectResult Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null || context.TopHUDController == null)
            {
                return ItemEffectResult.None;
            }

            int scoreBonus = 0;
            int healedLife = 0;
            if (context.PlayerHealth != null)
            {
                healedLife = context.PlayerHealth.Heal(definition.EffectValue);
                if (healedLife <= 0 && context.PlayerHealth.CurrentLife >= context.PlayerHealth.MaxLife)
                {
                    scoreBonus = definition.EffectValue * context.TopHUDController.FullHeartScoreBonusPerHeart;
                    context.TopHUDController.AddScore(scoreBonus);
                }
            }
            else
            {
                scoreBonus = context.TopHUDController.ApplyHealOrScoreBonus(definition.EffectValue);
            }

            context.TopHUDController.SetItemStatus(scoreBonus > 0 ? $"+{scoreBonus} SCORE" : $"+{definition.EffectValue} HP");
            return scoreBonus > 0
                ? new ItemEffectResult(ItemEffectOutcome.ScoreAdded, scoreBonus)
                : new ItemEffectResult(ItemEffectOutcome.LifeHealed, healedLife > 0 ? healedLife : definition.EffectValue);
        }
    }
}
