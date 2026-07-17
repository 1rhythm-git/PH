namespace PH.Core.Items
{
    public interface IItemIconProvider
    {
        ItemIconData GetIcon(ItemDefinition definition);
    }
}
