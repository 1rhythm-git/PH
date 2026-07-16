namespace PH.Core.Items
{
    public sealed class ItemEffectResolver
    {
        private readonly IItemEffect[] effects =
        {
            new AddScoreItemEffect(),
            new AddTimeItemEffect(),
            new AddMaxLifeItemEffect(),
            new HealHeartItemEffect(),
            new AddMoveSpeedItemEffect()
        };

        public ItemEffectResult Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null)
            {
                return ItemEffectResult.None;
            }

            for (int i = 0; i < effects.Length; i++)
            {
                IItemEffect effect = effects[i];
                if (effect != null && effect.CanExecute(definition))
                {
                    return effect.Execute(definition, context);
                }
            }

            context.TopHUDController?.SetItemStatus(definition.DisplayName);
            return ItemEffectResult.None;
        }
    }
}
