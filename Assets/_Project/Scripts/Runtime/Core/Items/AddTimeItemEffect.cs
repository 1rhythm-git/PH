namespace PH.Core.Items
{
    public sealed class AddTimeItemEffect : IItemEffect
    {
        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null && (definition.ItemType == ItemType.Time || definition.EffectKey == "add_time");
        }

        public void Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null || context.TopHUDController == null)
            {
                return;
            }

            context.TopHUDController.AddTime(definition.EffectValue);
            context.TopHUDController.SetItemStatus($"+{definition.EffectValue}s TIME");
        }
    }
}
