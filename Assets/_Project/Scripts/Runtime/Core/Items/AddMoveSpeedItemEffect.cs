using PH.Core.Player;
using UnityEngine;

namespace PH.Core.Items
{
    public sealed class AddMoveSpeedItemEffect : IItemEffect
    {
        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null && definition.EffectKey == ItemEffectKeys.AddMoveSpeedPercent;
        }

        public ItemEffectResult Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null)
            {
                return ItemEffectResult.None;
            }

            PlayerMotor playerMotor = context.PlayerMotor;
            float baseDurationSeconds = definition.EffectDurationSeconds > 0f
                ? definition.EffectDurationSeconds
                : definition.LifetimeSeconds;
            int passCount = Mathf.Max(1, context.RequiredPassCount);
            float durationSeconds = baseDurationSeconds * passCount;
            if (playerMotor == null)
            {
                context.TopHUDController?.SetItemStatus(definition.DisplayName);
                return ItemEffectResult.None;
            }

            float currentSpeed = playerMotor.AddTimedMoveSpeedPercentBonus(definition.EffectValue, durationSeconds);
            context.BuffVisualFeedback?.PlayBlink(durationSeconds);
            context.TopHUDController?.SetItemStatus($"+{definition.EffectValue}% SPEED  {durationSeconds:0.#}s  {currentSpeed:0.##}");
            return new ItemEffectResult(ItemEffectOutcome.MoveSpeedIncreased, definition.EffectValue);
        }
    }
}
