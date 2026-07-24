using UnityEngine;
using LootUp.Core.Items;

namespace LootUp.Core.Characters.Skills
{
    public sealed class AddTimeCharacterSkillEffect : ICharacterSkillEffect
    {
        public CharacterSkillEffectType EffectType => CharacterSkillEffectType.AddTime;

        public bool Execute(CharacterSkillEffectContext context)
        {
            if (context.Skill == null || context.TopHUDController == null)
            {
                return false;
            }

            float powerMultiplier = 1f + ArtifactEffectResolver.Resolve().CharacterSkillPowerBonusPercent * 0.01f;
            float addedSeconds = Mathf.Max(0f, context.Skill.P2 * powerMultiplier);
            if (addedSeconds <= 0f)
            {
                return false;
            }

            context.TopHUDController.AddTime(addedSeconds);
            context.TopHUDController.SetItemStatus($"SKILL +{addedSeconds:0.#}s TIME");
            return true;
        }
    }
}
