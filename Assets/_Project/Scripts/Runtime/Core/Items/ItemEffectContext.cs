using LootUp.Core.Player;
using LootUp.Core.UI;

namespace LootUp.Core.Items
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
            : this(topHUDController, playerHealth, playerMotor, buffVisualFeedback, requiredPassCount, scoreBonusPercent, null, string.Empty)
        {
        }

        public ItemEffectContext(
            TopHUDController topHUDController,
            PlayerHealth playerHealth,
            PlayerMotor playerMotor,
            PlayerBuffVisualFeedback buffVisualFeedback,
            int requiredPassCount,
            int scoreBonusPercent,
            RunItemEventRecorder runItemEventRecorder,
            string eventId)
        {
            TopHUDController = topHUDController;
            PlayerHealth = playerHealth;
            PlayerMotor = playerMotor;
            BuffVisualFeedback = buffVisualFeedback;
            RequiredPassCount = requiredPassCount;
            ScoreBonusPercent = scoreBonusPercent;
            RunItemEventRecorder = runItemEventRecorder;
            EventId = eventId ?? string.Empty;
        }

        public TopHUDController TopHUDController { get; }
        public PlayerHealth PlayerHealth { get; }
        public PlayerMotor PlayerMotor { get; }
        public PlayerBuffVisualFeedback BuffVisualFeedback { get; }
        public int RequiredPassCount { get; }
        public int ScoreBonusPercent { get; }
        public RunItemEventRecorder RunItemEventRecorder { get; }
        public string EventId { get; }
    }
}
