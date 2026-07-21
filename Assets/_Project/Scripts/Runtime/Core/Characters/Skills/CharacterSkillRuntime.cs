using PH.Core.Items;
using PH.Core.Player;
using PH.Core.UI;
using UnityEngine;

namespace PH.Core.Characters.Skills
{
    public sealed class CharacterSkillRuntime : MonoBehaviour
    {
        private readonly CharacterSkillEffectResolver effectResolver = new CharacterSkillEffectResolver();
        private CharacterDefinition characterDefinition;

        public CharacterSkillDefinition ActiveSkill => characterDefinition != null ? characterDefinition.CharacterSkill : null;

        public void Configure(CharacterDefinition definition)
        {
            characterDefinition = definition;
        }

        public bool TryActivate(
            ItemDefinition item,
            ItemEffectResult itemEffectResult,
            TopHUDController topHUDController,
            PlayerMotor playerMotor)
        {
            CharacterSkillDefinition skill = ActiveSkill;
            if (skill == null
                || item == null
                || !CharacterProgressionState.IsSkillUnlocked(characterDefinition)
                || !MatchesTrigger(skill.TriggerType, item.ItemType))
            {
                return false;
            }

            float activationChance = Mathf.Clamp(skill.P1, 0f, 100f) * 0.01f;
            if (activationChance <= 0f || Random.value > activationChance)
            {
                return false;
            }

            bool applied = effectResolver.Execute(new CharacterSkillEffectContext(
                skill,
                item,
                itemEffectResult,
                topHUDController,
                playerMotor));
            if (applied)
            {
                Debug.Log($"Character skill activated: {skill.SkillId}", this);
            }

            return applied;
        }

        private bool MatchesTrigger(CharacterSkillTriggerType triggerType, ItemType itemType)
        {
            switch (triggerType)
            {
                case CharacterSkillTriggerType.ScoreItemAcquired:
                    return itemType == ItemType.Score;
                case CharacterSkillTriggerType.TimeItemAcquired:
                    return itemType == ItemType.Time;
                case CharacterSkillTriggerType.HeartItemAcquired:
                    return itemType == ItemType.Heal;
                default:
                    return false;
            }
        }
    }
}
