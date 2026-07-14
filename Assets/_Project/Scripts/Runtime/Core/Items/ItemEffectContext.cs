using PH.Core.UI;

namespace PH.Core.Items
{
    public readonly struct ItemEffectContext
    {
        public ItemEffectContext(TopHUDController topHUDController)
        {
            TopHUDController = topHUDController;
        }

        public TopHUDController TopHUDController { get; }
    }
}
