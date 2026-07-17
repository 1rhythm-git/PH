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
            : this(topHUDController, playerHealth, playerMotor, buffVisualFeedback, 1, 0)
        {
        }

        public ItemEffectContext(TopHUDController topHUDController, PlayerHealth playerHealth, PlayerMotor playerMotor, PlayerBuffVisualFeedback buffVisualFeedback, int requiredPassCount, int scoreBonusPercent)
        {
            TopHUDController = topHUDController;
            PlayerHealth = playerHealth;
            PlayerMotor = playerMotor;
            BuffVisualFeedback = buffVisualFeedback;
            RequiredPassCount = requiredPassCount;
            ScoreBonusPercent = scoreBonusPercent;
        }

        public TopHUDController TopHUDController { get; }
        public PlayerHealth PlayerHealth { get; }
        public PlayerMotor PlayerMotor { get; }
        public PlayerBuffVisualFeedback BuffVisualFeedback { get; }
        public int RequiredPassCount { get; }
        public int ScoreBonusPercent { get; }
    }
}
