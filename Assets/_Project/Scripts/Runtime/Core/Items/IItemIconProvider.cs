namespace LootUp.Core.Items
{
    public interface IItemIconProvider
    {
        ItemIconData GetIcon(ItemDefinition definition);
    }
}
