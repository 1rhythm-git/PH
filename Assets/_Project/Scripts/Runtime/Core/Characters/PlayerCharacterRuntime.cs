using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PH.Core.Characters
{
    public sealed class PlayerCharacterRuntime : MonoBehaviour
    {
        [SerializeField]
        private CharacterDefinition characterDefinition;

        [SerializeField]
        [FormerlySerializedAs("boosterGauge")]
        private float feverGauge;

        private bool feverReadyLogged;
        private CharacterUpgradeModifiers upgradeModifiers;

        public event Action<float> FeverGaugeChanged;
        public event Action<float> BoosterGaugeChanged;

        public CharacterDefinition CharacterDefinition => characterDefinition;
        public float MoveSpeedColumnsPerSecond => characterDefinition != null
            ? characterDefinition.MoveSpeedColumnsPerSecond * (1f + upgradeModifiers.MoveSpeedBonusPercent * 0.01f)
            : 4f;
        public float PivotCooldownSeconds => characterDefinition != null ? characterDefinition.PivotCooldownSeconds : 0f;
        public int MaxLife => characterDefinition != null ? characterDefinition.MaxLife + upgradeModifiers.MaxLifeBonus : 3;
        public float InstantItemAcquireChance => characterDefinition != null
            ? Mathf.Clamp01(characterDefinition.InstantItemAcquireChance + upgradeModifiers.InstantItemAcquireChanceBonusPercent * 0.01f)
            : 0f;
        public float CollectionItemChanceBonusPercent => characterDefinition != null
            ? characterDefinition.CollectionItemChanceBonusPercent + upgradeModifiers.CollectionItemChanceBonusPercent
            : 0f;
        public CharacterProgressionSnapshot Progression => CharacterProgressionState.GetSnapshot(characterDefinition);
        public bool IsLevelSkillUnlocked => CharacterProgressionState.IsSkillUnlocked(characterDefinition);
        public float SkillItemPageSpawnChance => CharacterProgressionState.GetActiveSkillItemPageSpawnChance(characterDefinition);
        public float FeverGauge => feverGauge;
        public float FeverGaugeMax => characterDefinition != null ? characterDefinition.FeverGaugeMax : 100f;
        public float FeverGaugeNormalized => FeverGaugeMax <= 0f ? 0f : Mathf.Clamp01(feverGauge / FeverGaugeMax);
        public float BoosterGauge => FeverGauge;
        public float BoosterGaugeMax => FeverGaugeMax;
        public float BoosterGaugeNormalized => FeverGaugeNormalized;

        public void Configure(CharacterDefinition definition)
        {
            characterDefinition = definition;
            upgradeModifiers = CharacterUpgradeResolver.Resolve(definition);
            feverGauge = 0f;
            feverReadyLogged = false;
            NotifyFeverGaugeChanged();
        }

        public void AddMoveDistanceColumns(float movedColumns)
        {
            float gainPerColumn = characterDefinition != null ? characterDefinition.FeverGainPerColumn : 0f;
            AddFeverGauge(Mathf.Max(0f, movedColumns) * gainPerColumn);
        }

        public void AddPivotCharge()
        {
            float gain = characterDefinition != null ? characterDefinition.FeverGainPerPivot : 0f;
            AddFeverGauge(gain);
        }

        public bool RollInstantItemAcquire()
        {
            float chance = Mathf.Clamp01(InstantItemAcquireChance);
            return chance > 0f && UnityEngine.Random.value <= chance;
        }

        private void AddFeverGauge(float amount)
        {
            if (amount <= 0f || feverGauge >= FeverGaugeMax)
            {
                return;
            }

            feverGauge = Mathf.Min(FeverGaugeMax, feverGauge + amount);
            NotifyFeverGaugeChanged();

            if (!feverReadyLogged && feverGauge >= FeverGaugeMax)
            {
                feverReadyLogged = true;
                Debug.Log($"Fever Ready: {(characterDefinition != null ? characterDefinition.FeverBuffKey : "Undefined")}", this);
            }
        }

        private void NotifyFeverGaugeChanged()
        {
            FeverGaugeChanged?.Invoke(FeverGaugeNormalized);
            BoosterGaugeChanged?.Invoke(FeverGaugeNormalized);
        }
    }
}
