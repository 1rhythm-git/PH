namespace PH.Core.Items
{
    public sealed class AddScoreItemEffect : IItemEffect
    {
        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null && (definition.ItemType == ItemType.Score || definition.EffectKey == "add_score");
        }

        public void Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null || context.TopHUDController == null)
            {
                return;
            }

            context.TopHUDController.AddScore(definition.EffectValue);
            context.TopHUDController.SetItemStatus($"+{definition.EffectValue} SCORE");
        }
    }
}
