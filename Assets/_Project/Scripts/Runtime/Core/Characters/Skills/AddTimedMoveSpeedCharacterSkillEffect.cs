using UnityEngine;

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

            float currentSpeed = context.PlayerMotor.AddTimedMoveSpeedPercentBonus(context.Skill.P3, context.Skill.P2);
            context.TopHUDController?.SetItemStatus($"SKILL +{context.Skill.P3:0.#}% SPEED  {context.Skill.P2:0.#}s  {currentSpeed:0.##}");
            return true;
        }
    }
}
