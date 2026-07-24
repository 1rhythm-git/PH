using LootUp.Core.Player;
using UnityEngine;

namespace LootUp.Core.Items
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
            ArtifactModifiers artifactModifiers = ArtifactEffectResolver.Resolve();
            float durationSeconds = baseDurationSeconds
                * passCount
                * (1f + artifactModifiers.MoveSpeedDurationBonusPercent * 0.01f);
            float speedBonusPercent = definition.EffectValue
                * (1f + artifactModifiers.MoveSpeedPowerBonusPercent * 0.01f);
            if (playerMotor == null)
            {
                context.TopHUDController?.SetItemStatus(definition.DisplayName);
                return ItemEffectResult.None;
            }

            float currentSpeed = playerMotor.AddTimedMoveSpeedPercentBonus(speedBonusPercent, durationSeconds);
            float activeBonusPercent = playerMotor.MoveSpeedBonusPercent;
            // (변경) 하위 아이템으로 지속시간만 갱신한 경우에도 실제 유지 중인 능력치를 표시한다.
            context.TopHUDController?.SetItemStatus($"+{activeBonusPercent:0}% SPEED  {durationSeconds:0.#}s  {currentSpeed:0.##}");
            return new ItemEffectResult(ItemEffectOutcome.MoveSpeedIncreased, Mathf.RoundToInt(speedBonusPercent));
        }
    }
}
