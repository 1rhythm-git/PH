using LootUp.Core.Items;
using LootUp.Core.Player;
using LootUp.Core.UI;
using UnityEngine;

namespace LootUp.Core.Characters.Skills
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
                ShowActivationFeedback(skill, true, playerMotor);
                Debug.Log($"Character skill activated: {skill.SkillId}", this);
            }

            return applied;
        }

        public void ShowActivationFeedback(bool stackAboveItemFeedback)
        {
            ShowActivationFeedback(ActiveSkill, stackAboveItemFeedback, GetComponent<PlayerMotor>());
        }

        private void ShowActivationFeedback(CharacterSkillDefinition skill, bool stackAboveItemFeedback, PlayerMotor playerMotor)
        {
            if (skill == null)
            {
                return;
            }

            GameObject feedbackOwner = playerMotor != null ? playerMotor.gameObject : gameObject;
            PlayerItemPickupFeedback feedback = feedbackOwner.GetComponent<PlayerItemPickupFeedback>();
            if (feedback == null)
            {
                feedback = feedbackOwner.AddComponent<PlayerItemPickupFeedback>();
            }

            feedback.ShowSkillActivation(stackAboveItemFeedback);
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
