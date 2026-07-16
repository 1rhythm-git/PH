using PH.Core.Player;
using PH.Core.UI;

namespace PH.Core.Items
{
    public readonly struct ItemEffectContext
    {
        public ItemEffectContext(TopHUDController topHUDController)
            : this(topHUDController, null)
        {
        }

        public ItemEffectContext(TopHUDController topHUDController, PlayerHealth playerHealth)
        {
            TopHUDController = topHUDController;
            PlayerHealth = playerHealth;
        }

        public TopHUDController TopHUDController { get; }
        public PlayerHealth PlayerHealth { get; }
    }
}
