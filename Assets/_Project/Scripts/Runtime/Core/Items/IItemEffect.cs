namespace LootUp.Core.Items
{
    public interface IItemEffect
    {
        bool CanExecute(ItemDefinition definition);
        ItemEffectResult Execute(ItemDefinition definition, ItemEffectContext context);
    }
}
