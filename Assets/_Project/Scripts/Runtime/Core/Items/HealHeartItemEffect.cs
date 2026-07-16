namespace PH.Core.Items
{
    public sealed class HealHeartItemEffect : IItemEffect
    {
        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null && (definition.ItemType == ItemType.Heal || definition.EffectKey == "heal_heart");
        }

        public void Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null || context.TopHUDController == null)
            {
                return;
            }

            int scoreBonus = 0;
            if (context.PlayerHealth != null)
            {
                int healAmount = context.PlayerHealth.Heal(definition.EffectValue);
                if (healAmount <= 0 && context.PlayerHealth.CurrentLife >= context.PlayerHealth.MaxLife)
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
        }
    }
}
