using UnityEngine;
using UnityEngine.Serialization;

namespace PH.Core.Characters
{
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "PH/Characters/Character Definition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [SerializeField]
        private string characterId = "default";

        [SerializeField]
        private string displayName = "Default";

        [SerializeField]
        private Sprite portraitSprite;

        [SerializeField]
        private Color bodyColor = new Color(0.95f, 0.78f, 0.22f, 1f);

        [SerializeField]
        private Color outlineColor = new Color(0.05f, 0.05f, 0.06f, 0.85f);

        [SerializeField]
        private CharacterBodyShape bodyShape = CharacterBodyShape.Square;

        [SerializeField]
        private Sprite[] idleSprites;

        [SerializeField]
        private Sprite[] walkSprites;

        [SerializeField]
        private Sprite[] runSprites;

        [SerializeField]
        private float[] idleFrameScales;

        [SerializeField]
        private float[] walkFrameScales;

        [SerializeField]
        private float[] runFrameScales;

        [SerializeField]
        private float animationFramesPerSecond = 6f;

        [SerializeField]
        private Vector2 spriteVisualScale = Vector2.one;

        [SerializeField]
        private float moveSpeedColumnsPerSecond = 4f;

        [SerializeField]
        private float pivotCooldownSeconds;

        [SerializeField]
        [FormerlySerializedAs("boosterGaugeMax")]
        private float feverGaugeMax = 100f;

        [SerializeField]
        [FormerlySerializedAs("boosterGainPerColumn")]
        private float feverGainPerColumn = 8f;

        [SerializeField]
        [FormerlySerializedAs("boosterGainPerPivot")]
        private float feverGainPerPivot = 12f;

        [SerializeField]
        [FormerlySerializedAs("boosterBuffKey")]
        private string feverBuffKey = "Undefined";

        [SerializeField]
        private int maxLife = 3;

        [SerializeField, Range(0f, 1f)]
        private float instantItemAcquireChance;

        [SerializeField, Min(0f)]
        private float collectionItemChanceBonusPercent;

        [SerializeField]
        private CharacterUpgradeDefinition[] collectionUpgrades;

        [SerializeField]
        private int[] requiredExperienceByLevel = { 100, 150, 225, 325, 450, 600, 800, 1050, 1350 };

        [SerializeField]
        private string unlockableSkillId = "skill_item_page_chance";

        [SerializeField]
        private string unlockableSkillName = "Item Scout";

        [SerializeField]
        [TextArea(2, 4)]
        private string unlockableSkillDescription = "Increases the chance of a Time or Speed item appearing on each page.";

        [SerializeField]
        private int skillUnlockLevel = 3;

        [SerializeField, Range(0f, 1f)]
        private float skillItemPageSpawnChance = 0.15f;

        public string CharacterId => characterId;
        public string DisplayName => displayName;
        public Sprite PortraitSprite => portraitSprite;
        public Color BodyColor => bodyColor;
        public Color OutlineColor => outlineColor;
        public CharacterBodyShape BodyShape => bodyShape;
        public Sprite[] IdleSprites => idleSprites;
        public Sprite[] WalkSprites => walkSprites;
        public Sprite[] RunSprites => runSprites;
        public float AnimationFramesPerSecond => animationFramesPerSecond;
        public Vector2 SpriteVisualScale => spriteVisualScale;
        public float MoveSpeedColumnsPerSecond => moveSpeedColumnsPerSecond;
        public float PivotCooldownSeconds => pivotCooldownSeconds;
        public float FeverGaugeMax => feverGaugeMax;
        public float FeverGainPerColumn => feverGainPerColumn;
        public float FeverGainPerPivot => feverGainPerPivot;
        public string FeverBuffKey => feverBuffKey;
        public float BoosterGaugeMax => FeverGaugeMax;
        public float BoosterGainPerColumn => FeverGainPerColumn;
        public float BoosterGainPerPivot => FeverGainPerPivot;
        public string BoosterBuffKey => FeverBuffKey;
        public int MaxLife => maxLife;
        public float InstantItemAcquireChance => instantItemAcquireChance;
        public float CollectionItemChanceBonusPercent => Mathf.Max(0f, collectionItemChanceBonusPercent);
        public CharacterUpgradeDefinition[] CollectionUpgrades => collectionUpgrades;
        public int MaxCharacterLevel => Mathf.Max(1, (requiredExperienceByLevel?.Length ?? 0) + 1);
        public string UnlockableSkillId => unlockableSkillId;
        public string UnlockableSkillName => unlockableSkillName;
        public string UnlockableSkillDescription => unlockableSkillDescription;
        public int SkillUnlockLevel => Mathf.Clamp(skillUnlockLevel, 1, MaxCharacterLevel);
        public float SkillItemPageSpawnChance => Mathf.Clamp01(skillItemPageSpawnChance);

        public float GetIdleFrameScale(int frameIndex)
        {
            return GetFrameScale(idleFrameScales, frameIndex);
        }

        public float GetWalkFrameScale(int frameIndex)
        {
            return GetFrameScale(walkFrameScales, frameIndex);
        }

        public float GetRunFrameScale(int frameIndex)
        {
            return GetFrameScale(runFrameScales, frameIndex);
        }

        // (추가) 캐릭터 테이블의 현재 레벨 기준 필요 XP를 반환한다. 최대 레벨은 0을 반환한다.
        public int GetRequiredExperienceForLevel(int currentLevel)
        {
            int index = Mathf.Max(1, currentLevel) - 1;
            if (requiredExperienceByLevel == null || index < 0 || index >= requiredExperienceByLevel.Length)
            {
                return 0;
            }

            return Mathf.Max(1, requiredExperienceByLevel[index]);
        }

        private float GetFrameScale(float[] frameScales, int frameIndex)
        {
            if (frameScales == null || frameIndex < 0 || frameIndex >= frameScales.Length)
            {
                return 1f;
            }

            return Mathf.Max(0.01f, frameScales[frameIndex]);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            moveSpeedColumnsPerSecond = Mathf.Max(0f, moveSpeedColumnsPerSecond);
            animationFramesPerSecond = Mathf.Max(1f, animationFramesPerSecond);
            spriteVisualScale.x = Mathf.Max(0.01f, spriteVisualScale.x);
            spriteVisualScale.y = Mathf.Max(0.01f, spriteVisualScale.y);
            ClampFrameScales(idleFrameScales);
            ClampFrameScales(walkFrameScales);
            ClampFrameScales(runFrameScales);
            pivotCooldownSeconds = Mathf.Max(0f, pivotCooldownSeconds);
            feverGaugeMax = Mathf.Max(1f, feverGaugeMax);
            feverGainPerColumn = Mathf.Max(0f, feverGainPerColumn);
            feverGainPerPivot = Mathf.Max(0f, feverGainPerPivot);
            maxLife = Mathf.Max(1, maxLife);
            instantItemAcquireChance = Mathf.Clamp01(instantItemAcquireChance);
            collectionItemChanceBonusPercent = Mathf.Max(0f, collectionItemChanceBonusPercent);
            skillUnlockLevel = Mathf.Clamp(skillUnlockLevel, 1, MaxCharacterLevel);
            skillItemPageSpawnChance = Mathf.Clamp01(skillItemPageSpawnChance);

            if (requiredExperienceByLevel != null)
            {
                for (int i = 0; i < requiredExperienceByLevel.Length; i++)
                {
                    requiredExperienceByLevel[i] = Mathf.Max(1, requiredExperienceByLevel[i]);
                }
            }
        }

        private void ClampFrameScales(float[] frameScales)
        {
            if (frameScales == null)
            {
                return;
            }

            for (int i = 0; i < frameScales.Length; i++)
            {
                frameScales[i] = Mathf.Max(0.01f, frameScales[i]);
            }
        }
#endif
    }
}
