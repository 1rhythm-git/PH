using UnityEngine;

namespace LootUp.Core.Items
{
    public readonly struct ArtifactModifiers
    {
        public ArtifactModifiers(
            float resultExperienceBonusPercent,
            float resultScoreBonusPercent,
            float moveSpeedPowerBonusPercent,
            float moveSpeedDurationBonusPercent,
            float scoreItemDoubleChancePercent,
            float timeItemDoubleChancePercent,
            float characterSkillPowerBonusPercent,
            float characterCoinChanceBonusPercent)
        {
            ResultExperienceBonusPercent = resultExperienceBonusPercent;
            ResultScoreBonusPercent = resultScoreBonusPercent;
            MoveSpeedPowerBonusPercent = moveSpeedPowerBonusPercent;
            MoveSpeedDurationBonusPercent = moveSpeedDurationBonusPercent;
            ScoreItemDoubleChancePercent = scoreItemDoubleChancePercent;
            TimeItemDoubleChancePercent = timeItemDoubleChancePercent;
            CharacterSkillPowerBonusPercent = characterSkillPowerBonusPercent;
            CharacterCoinChanceBonusPercent = characterCoinChanceBonusPercent;
        }

        public float ResultExperienceBonusPercent { get; }
        public float ResultScoreBonusPercent { get; }
        public float MoveSpeedPowerBonusPercent { get; }
        public float MoveSpeedDurationBonusPercent { get; }
        public float ScoreItemDoubleChancePercent { get; }
        public float TimeItemDoubleChancePercent { get; }
        public float CharacterSkillPowerBonusPercent { get; }
        public float CharacterCoinChanceBonusPercent { get; }
    }

    public static class ArtifactEffectResolver
    {
        public static ArtifactModifiers Resolve()
        {
            float resultExperience = 0f;
            float resultScore = 0f;
            float moveSpeedPower = 0f;
            float moveSpeedDuration = 0f;
            float scoreDoubleChance = 0f;
            float timeDoubleChance = 0f;
            float skillPower = 0f;
            float characterCoinChance = 0f;

            var effects = ArtifactCatalog.Instance.Effects;
            for (int i = 0; i < effects.Count; i++)
            {
                ArtifactEffectDefinition effect = effects[i];
                if (!effect.IsActive)
                {
                    continue;
                }

                switch (effect.EffectType)
                {
                    case ArtifactEffectType.ResultExperienceBonusPercent:
                        resultExperience += effect.ValuePercent;
                        break;
                    case ArtifactEffectType.ResultScoreBonusPercent:
                        resultScore += effect.ValuePercent;
                        break;
                    case ArtifactEffectType.MoveSpeedPowerBonusPercent:
                        moveSpeedPower += effect.ValuePercent;
                        break;
                    case ArtifactEffectType.MoveSpeedDurationBonusPercent:
                        moveSpeedDuration += effect.ValuePercent;
                        break;
                    case ArtifactEffectType.ScoreItemDoubleChancePercent:
                        scoreDoubleChance += effect.ValuePercent;
                        break;
                    case ArtifactEffectType.TimeItemDoubleChancePercent:
                        timeDoubleChance += effect.ValuePercent;
                        break;
                    case ArtifactEffectType.CharacterSkillPowerBonusPercent:
                        skillPower += effect.ValuePercent;
                        break;
                    case ArtifactEffectType.CharacterCoinChanceBonusPercent:
                        characterCoinChance += effect.ValuePercent;
                        break;
                }
            }

            return new ArtifactModifiers(
                resultExperience,
                resultScore,
                moveSpeedPower,
                moveSpeedDuration,
                Mathf.Clamp(scoreDoubleChance, 0f, 100f),
                Mathf.Clamp(timeDoubleChance, 0f, 100f),
                skillPower,
                characterCoinChance);
        }

        public static bool RollPercent(float chancePercent)
        {
            return chancePercent > 0f && Random.value <= Mathf.Clamp01(chancePercent * 0.01f);
        }
    }
}
