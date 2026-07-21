using UnityEngine;

namespace PH.Core.Characters.Skills
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

            float addedSeconds = Mathf.Max(0f, context.Skill.P2);
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
