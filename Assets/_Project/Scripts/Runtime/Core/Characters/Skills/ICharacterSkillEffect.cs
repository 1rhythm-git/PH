using LootUp.Core.Items;
using LootUp.Core.Player;
using LootUp.Core.UI;

namespace LootUp.Core.Characters.Skills
{
    public readonly struct CharacterSkillEffectContext
    {
        public CharacterSkillEffectContext(
            CharacterSkillDefinition skill,
            ItemDefinition item,
            ItemEffectResult itemEffectResult,
            TopHUDController topHUDController,
            PlayerMotor playerMotor)
        {
            Skill = skill;
            Item = item;
            ItemEffectResult = itemEffectResult;
            TopHUDController = topHUDController;
            PlayerMotor = playerMotor;
        }

        public CharacterSkillDefinition Skill { get; }
        public ItemDefinition Item { get; }
        public ItemEffectResult ItemEffectResult { get; }
        public TopHUDController TopHUDController { get; }
        public PlayerMotor PlayerMotor { get; }
    }

    public interface ICharacterSkillEffect
    {
        CharacterSkillEffectType EffectType { get; }
        bool Execute(CharacterSkillEffectContext context);
    }
}
