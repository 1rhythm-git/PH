namespace PH.Core.Characters.Skills
{
    public sealed class CharacterSkillEffectResolver
    {
        private readonly ICharacterSkillEffect[] effects =
        {
            new AddBonusScoreCharacterSkillEffect(),
            new AddTimedMoveSpeedCharacterSkillEffect(),
            new AddTimeCharacterSkillEffect()
        };

        public bool Execute(CharacterSkillEffectContext context)
        {
            if (context.Skill == null)
            {
                return false;
            }

            for (int i = 0; i < effects.Length; i++)
            {
                ICharacterSkillEffect effect = effects[i];
                if (effect != null && effect.EffectType == context.Skill.EffectType)
                {
                    return effect.Execute(context);
                }
            }

            return false;
        }
    }
}
