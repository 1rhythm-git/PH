using UnityEngine;
using LootUp.Core.Items;

namespace LootUp.Core.Characters.Skills
{
    public sealed class AddBonusScoreCharacterSkillEffect : ICharacterSkillEffect
    {
        public CharacterSkillEffectType EffectType => CharacterSkillEffectType.AddBonusScorePercent;

        public bool Execute(CharacterSkillEffectContext context)
        {
            if (context.Skill == null || context.TopHUDController == null)
            {
                return false;
            }

            int acquiredScore = Mathf.Max(0, context.ItemEffectResult.Value);
            float powerMultiplier = 1f + ArtifactEffectResolver.Resolve().CharacterSkillPowerBonusPercent * 0.01f;
            int bonusScore = Mathf.FloorToInt(acquiredScore * context.Skill.P2 * powerMultiplier * 0.01f);
            if (bonusScore <= 0)
            {
                return false;
            }

            context.TopHUDController.AddScore(bonusScore);
            context.TopHUDController.SetItemStatus($"SKILL +{bonusScore} SCORE");
            return true;
        }
    }
}
