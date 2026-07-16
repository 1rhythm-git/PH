namespace PH.Core.Items
{
    public sealed class ItemEffectResolver
    {
        private readonly IItemEffect[] effects =
        {
            new AddScoreItemEffect(),
            new AddTimeItemEffect(),
            new HealHeartItemEffect(),
            new AddMoveSpeedItemEffect()
        };

        public void Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null)
            {
                return;
            }

            for (int i = 0; i < effects.Length; i++)
            {
                IItemEffect effect = effects[i];
                if (effect != null && effect.CanExecute(definition))
                {
                    effect.Execute(definition, context);
                    return;
                }
            }

            context.TopHUDController?.SetItemStatus(definition.DisplayName);
        }
    }
}
