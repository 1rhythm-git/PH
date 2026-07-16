using UnityEngine;

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
        private float animationFramesPerSecond = 6f;

        [SerializeField]
        private Vector2 spriteVisualScale = Vector2.one;

        [SerializeField]
        private float moveSpeedColumnsPerSecond = 4f;

        [SerializeField]
        private float pivotCooldownSeconds;

        [SerializeField]
        private float boosterGaugeMax = 100f;

        [SerializeField]
        private float boosterGainPerColumn = 8f;

        [SerializeField]
        private float boosterGainPerPivot = 12f;

        [SerializeField]
        private string boosterBuffKey = "Undefined";

        [SerializeField]
        private int maxLife = 3;

        [SerializeField, Range(0f, 1f)]
        private float instantItemAcquireChance;

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
        public float BoosterGaugeMax => boosterGaugeMax;
        public float BoosterGainPerColumn => boosterGainPerColumn;
        public float BoosterGainPerPivot => boosterGainPerPivot;
        public string BoosterBuffKey => boosterBuffKey;
        public int MaxLife => maxLife;
        public float InstantItemAcquireChance => instantItemAcquireChance;

#if UNITY_EDITOR
        private void OnValidate()
        {
            moveSpeedColumnsPerSecond = Mathf.Max(0f, moveSpeedColumnsPerSecond);
            animationFramesPerSecond = Mathf.Max(1f, animationFramesPerSecond);
            spriteVisualScale.x = Mathf.Max(0.01f, spriteVisualScale.x);
            spriteVisualScale.y = Mathf.Max(0.01f, spriteVisualScale.y);
            pivotCooldownSeconds = Mathf.Max(0f, pivotCooldownSeconds);
            boosterGaugeMax = Mathf.Max(1f, boosterGaugeMax);
            boosterGainPerColumn = Mathf.Max(0f, boosterGainPerColumn);
            boosterGainPerPivot = Mathf.Max(0f, boosterGainPerPivot);
            maxLife = Mathf.Max(1, maxLife);
            instantItemAcquireChance = Mathf.Clamp01(instantItemAcquireChance);
        }
#endif
    }
}
