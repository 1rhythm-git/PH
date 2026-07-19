using PH.Core.Items;
using UnityEngine;

namespace PH.Core.Characters
{
    [System.Serializable]
    public sealed class CharacterUpgradeLevelDefinition
    {
        [SerializeField]
        private CollectionCost[] costs;

        [SerializeField, Min(0f)]
        private float moveSpeedBonusPercent;

        [SerializeField, Min(0)]
        private int maxLifeBonus;

        [SerializeField, Min(0f)]
        private float instantItemAcquireChanceBonusPercent;

        [SerializeField, Min(0f)]
        private float collectionItemChanceBonusPercent;

        public CollectionCost[] Costs => costs;
        public float MoveSpeedBonusPercent => Mathf.Max(0f, moveSpeedBonusPercent);
        public int MaxLifeBonus => Mathf.Max(0, maxLifeBonus);
        public float InstantItemAcquireChanceBonusPercent => Mathf.Max(0f, instantItemAcquireChanceBonusPercent);
        public float CollectionItemChanceBonusPercent => Mathf.Max(0f, collectionItemChanceBonusPercent);
    }

    public readonly struct CharacterUpgradeModifiers
    {
        public CharacterUpgradeModifiers(float moveSpeedBonusPercent, int maxLifeBonus, float instantItemAcquireChanceBonusPercent, float collectionItemChanceBonusPercent)
        {
            MoveSpeedBonusPercent = Mathf.Max(0f, moveSpeedBonusPercent);
            MaxLifeBonus = Mathf.Max(0, maxLifeBonus);
            InstantItemAcquireChanceBonusPercent = Mathf.Max(0f, instantItemAcquireChanceBonusPercent);
            CollectionItemChanceBonusPercent = Mathf.Max(0f, collectionItemChanceBonusPercent);
        }

        public float MoveSpeedBonusPercent { get; }
        public int MaxLifeBonus { get; }
        public float InstantItemAcquireChanceBonusPercent { get; }
        public float CollectionItemChanceBonusPercent { get; }

        public CharacterUpgradeModifiers Add(CharacterUpgradeLevelDefinition level)
        {
            return level == null
                ? this
                : new CharacterUpgradeModifiers(
                    MoveSpeedBonusPercent + level.MoveSpeedBonusPercent,
                    MaxLifeBonus + level.MaxLifeBonus,
                    InstantItemAcquireChanceBonusPercent + level.InstantItemAcquireChanceBonusPercent,
                    CollectionItemChanceBonusPercent + level.CollectionItemChanceBonusPercent);
        }
    }

    [CreateAssetMenu(fileName = "CharacterUpgradeDefinition", menuName = "PH/Characters/Character Upgrade Definition")]
    public sealed class CharacterUpgradeDefinition : ScriptableObject
    {
        [SerializeField]
        private string upgradeId;

        [SerializeField]
        private CharacterUpgradeLevelDefinition[] levels;

        public string UpgradeId => upgradeId;
        public int MaxLevel => levels?.Length ?? 0;

        public CharacterUpgradeResult TryPurchase(string characterId)
        {
            int currentLevel = ItemCollectionManager.GetCharacterUpgradeLevel(characterId, upgradeId);
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(upgradeId) || currentLevel < 0 || currentLevel >= MaxLevel)
            {
                return new CharacterUpgradeResult(CharacterUpgradeStatus.InvalidRequest, currentLevel, currentLevel);
            }

            CharacterUpgradeLevelDefinition nextLevel = levels[currentLevel];
            return ItemCollectionManager.TryApplyCharacterUpgrade(characterId, upgradeId, nextLevel?.Costs);
        }

        public CharacterUpgradeModifiers GetActiveModifiers(string characterId)
        {
            int activeLevel = Mathf.Clamp(ItemCollectionManager.GetCharacterUpgradeLevel(characterId, upgradeId), 0, MaxLevel);
            CharacterUpgradeModifiers modifiers = default;
            for (int i = 0; i < activeLevel; i++)
            {
                modifiers = modifiers.Add(levels[i]);
            }

            return modifiers;
        }
    }

    public static class CharacterUpgradeResolver
    {
        public static CharacterUpgradeModifiers Resolve(CharacterDefinition definition)
        {
            CharacterUpgradeModifiers modifiers = default;
            if (definition == null || definition.CollectionUpgrades == null)
            {
                return modifiers;
            }

            for (int i = 0; i < definition.CollectionUpgrades.Length; i++)
            {
                CharacterUpgradeDefinition upgrade = definition.CollectionUpgrades[i];
                if (upgrade == null)
                {
                    continue;
                }

                CharacterUpgradeModifiers activeModifiers = upgrade.GetActiveModifiers(definition.CharacterId);
                modifiers = new CharacterUpgradeModifiers(
                    modifiers.MoveSpeedBonusPercent + activeModifiers.MoveSpeedBonusPercent,
                    modifiers.MaxLifeBonus + activeModifiers.MaxLifeBonus,
                    modifiers.InstantItemAcquireChanceBonusPercent + activeModifiers.InstantItemAcquireChanceBonusPercent,
                    modifiers.CollectionItemChanceBonusPercent + activeModifiers.CollectionItemChanceBonusPercent);
            }

            return modifiers;
        }
    }
}
