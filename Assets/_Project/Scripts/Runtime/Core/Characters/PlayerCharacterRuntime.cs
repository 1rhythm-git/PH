using System;
using UnityEngine;

namespace PH.Core.Characters
{
    public sealed class PlayerCharacterRuntime : MonoBehaviour
    {
        [SerializeField]
        private CharacterDefinition characterDefinition;

        [SerializeField]
        private float boosterGauge;

        private bool boosterReadyLogged;

        public event Action<float> BoosterGaugeChanged;

        public CharacterDefinition CharacterDefinition => characterDefinition;
        public float MoveSpeedColumnsPerSecond => characterDefinition != null ? characterDefinition.MoveSpeedColumnsPerSecond : 4f;
        public float PivotCooldownSeconds => characterDefinition != null ? characterDefinition.PivotCooldownSeconds : 0f;
        public int MaxLife => characterDefinition != null ? characterDefinition.MaxLife : 3;
        public float InstantItemAcquireChance => characterDefinition != null ? characterDefinition.InstantItemAcquireChance : 0f;
        public float BoosterGauge => boosterGauge;
        public float BoosterGaugeMax => characterDefinition != null ? characterDefinition.BoosterGaugeMax : 100f;
        public float BoosterGaugeNormalized => BoosterGaugeMax <= 0f ? 0f : Mathf.Clamp01(boosterGauge / BoosterGaugeMax);

        public void Configure(CharacterDefinition definition)
        {
            characterDefinition = definition;
            boosterGauge = 0f;
            boosterReadyLogged = false;
            BoosterGaugeChanged?.Invoke(BoosterGaugeNormalized);
        }

        public void AddMoveDistanceColumns(float movedColumns)
        {
            float gainPerColumn = characterDefinition != null ? characterDefinition.BoosterGainPerColumn : 0f;
            AddBoosterGauge(Mathf.Max(0f, movedColumns) * gainPerColumn);
        }

        public void AddPivotCharge()
        {
            float gain = characterDefinition != null ? characterDefinition.BoosterGainPerPivot : 0f;
            AddBoosterGauge(gain);
        }

        public bool RollInstantItemAcquire()
        {
            float chance = Mathf.Clamp01(InstantItemAcquireChance);
            return chance > 0f && UnityEngine.Random.value <= chance;
        }

        private void AddBoosterGauge(float amount)
        {
            if (amount <= 0f || boosterGauge >= BoosterGaugeMax)
            {
                return;
            }

            boosterGauge = Mathf.Min(BoosterGaugeMax, boosterGauge + amount);
            BoosterGaugeChanged?.Invoke(BoosterGaugeNormalized);

            if (!boosterReadyLogged && boosterGauge >= BoosterGaugeMax)
            {
                boosterReadyLogged = true;
                Debug.Log($"Booster Ready: {(characterDefinition != null ? characterDefinition.BoosterBuffKey : "Undefined")}", this);
            }
        }
    }
}
