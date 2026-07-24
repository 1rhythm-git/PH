using UnityEngine;
using LootUp.Core.Items;

namespace LootUp.Core.Characters.Skills
{
    public sealed class AddTimedMoveSpeedCharacterSkillEffect : ICharacterSkillEffect
    {
        public CharacterSkillEffectType EffectType => CharacterSkillEffectType.AddTimedMoveSpeedPercent;

        public bool Execute(CharacterSkillEffectContext context)
        {
            if (context.Skill == null || context.PlayerMotor == null || context.Skill.P2 <= 0f || context.Skill.P3 <= 0f)
            {
                return false;
            }

            float powerMultiplier = 1f + ArtifactEffectResolver.Resolve().CharacterSkillPowerBonusPercent * 0.01f;
            float speedBonusPercent = context.Skill.P3 * powerMultiplier;
            float currentSpeed = context.PlayerMotor.AddTimedMoveSpeedPercentBonus(speedBonusPercent, context.Skill.P2);
            context.TopHUDController?.SetItemStatus($"SKILL +{speedBonusPercent:0.#}% SPEED  {context.Skill.P2:0.#}s  {currentSpeed:0.##}");
            return true;
        }
    }
}
