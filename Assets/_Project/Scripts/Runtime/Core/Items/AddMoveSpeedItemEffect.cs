using PH.Core.Player;
using UnityEngine;

namespace PH.Core.Items
{
    public sealed class AddMoveSpeedItemEffect : IItemEffect
    {
        private const string EffectKey = "add_move_speed_percent";

        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null && definition.EffectKey == EffectKey;
        }

        public void Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null)
            {
                return;
            }

            PlayerMotor playerMotor = context.PlayerMotor;
            float durationSeconds = definition.EffectDurationSeconds > 0f
                ? definition.EffectDurationSeconds
                : definition.LifetimeSeconds;
            if (playerMotor == null)
            {
                context.TopHUDController?.SetItemStatus(definition.DisplayName);
                return;
            }

            float currentSpeed = playerMotor.AddTimedMoveSpeedPercentBonus(definition.EffectValue, durationSeconds);
            context.BuffVisualFeedback?.PlayBlink(durationSeconds);
            context.TopHUDController?.SetItemStatus($"+{definition.EffectValue}% SPEED  {durationSeconds:0.#}s  {currentSpeed:0.##}");
        }
    }
}
