using PH.Core.Player;
using PH.Core.UI;

namespace PH.Core.Items
{
    public readonly struct ItemEffectContext
    {
        public ItemEffectContext(TopHUDController topHUDController)
            : this(topHUDController, null, null)
        {
        }

        public ItemEffectContext(TopHUDController topHUDController, PlayerHealth playerHealth)
            : this(topHUDController, playerHealth, null)
        {
        }

        public ItemEffectContext(TopHUDController topHUDController, PlayerHealth playerHealth, PlayerMotor playerMotor)
            : this(topHUDController, playerHealth, playerMotor, null)
        {
        }

        public ItemEffectContext(TopHUDController topHUDController, PlayerHealth playerHealth, PlayerMotor playerMotor, PlayerBuffVisualFeedback buffVisualFeedback)
        {
            TopHUDController = topHUDController;
            PlayerHealth = playerHealth;
            PlayerMotor = playerMotor;
            BuffVisualFeedback = buffVisualFeedback;
        }

        public TopHUDController TopHUDController { get; }
        public PlayerHealth PlayerHealth { get; }
        public PlayerMotor PlayerMotor { get; }
        public PlayerBuffVisualFeedback BuffVisualFeedback { get; }
    }
}
