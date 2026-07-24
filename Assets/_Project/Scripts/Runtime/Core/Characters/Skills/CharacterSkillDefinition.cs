using UnityEngine;

namespace LootUp.Core.Characters.Skills
{
    public enum CharacterSkillTriggerType
    {
        ScoreItemAcquired,
        TimeItemAcquired,
        HeartItemAcquired
    }

    public enum CharacterSkillEffectType
    {
        AddBonusScorePercent,
        AddTimedMoveSpeedPercent,
        AddTime
    }

    [CreateAssetMenu(fileName = "CharacterSkillDefinition", menuName = "LootUp/Characters/Character Skill Definition")]
    public sealed class CharacterSkillDefinition : ScriptableObject
    {
        [SerializeField]
        private string skillId = "Undefined";

        [SerializeField]
        private string displayName = "Skill";

        [SerializeField, Min(1)]
        private int unlockLevel = 1;

        [SerializeField]
        private CharacterSkillTriggerType triggerType;

        [SerializeField]
        private CharacterSkillEffectType effectType;

        [SerializeField]
        [TextArea(2, 4)]
        private string description;

        [Header("Skill Parameters")]
        [SerializeField, Range(0f, 100f)]
        [Tooltip("P1: 발동 확률(%)")]
        private float p1;

        [SerializeField, Min(0f)]
        [Tooltip("P2: 효과별 첫 번째 수치")]
        private float p2;

        [SerializeField, Min(0f)]
        [Tooltip("P3: 효과별 두 번째 수치")]
        private float p3;

        [SerializeField, Min(0f)]
        [Tooltip("P4: 확장용 예약 수치")]
        private float p4;

        [SerializeField, Min(0f)]
        [Tooltip("P5: 확장용 예약 수치")]
        private float p5;

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public int UnlockLevel => Mathf.Max(1, unlockLevel);
        public CharacterSkillTriggerType TriggerType => triggerType;
        public CharacterSkillEffectType EffectType => effectType;
        public string Description => description;
        public float P1 => Mathf.Clamp(p1, 0f, 100f);
        public float P2 => Mathf.Max(0f, p2);
        public float P3 => Mathf.Max(0f, p3);
        public float P4 => Mathf.Max(0f, p4);
        public float P5 => Mathf.Max(0f, p5);

#if UNITY_EDITOR
        private void OnValidate()
        {
            unlockLevel = Mathf.Max(1, unlockLevel);
            p1 = Mathf.Clamp(p1, 0f, 100f);
            p2 = Mathf.Max(0f, p2);
            p3 = Mathf.Max(0f, p3);
            p4 = Mathf.Max(0f, p4);
            p5 = Mathf.Max(0f, p5);
        }
#endif
    }
}
