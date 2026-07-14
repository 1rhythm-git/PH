namespace PH.Core.Items
{
    public interface IItemEffect
    {
        bool CanExecute(ItemDefinition definition);
        void Execute(ItemDefinition definition, ItemEffectContext context);
    }
}
