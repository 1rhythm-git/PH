using System;
using UnityEngine;

namespace PH.Core.Items
{
    [Serializable]
    public sealed class ItemDefinition
    {
        [SerializeField]
        private string itemId;

        [SerializeField]
        private string serverItemId;

        [SerializeField]
        private string tableVersion;

        [SerializeField]
        private bool enabled;

        [SerializeField]
        private string displayName;

        [SerializeField]
        private ItemType itemType;

        [SerializeField]
        private string iconKey;

        [SerializeField]
        private int minFloor;

        [SerializeField]
        private int maxFloor;

        [SerializeField]
        private int spawnWeight;

        [SerializeField]
        private int requiredPassCount;

        [SerializeField]
        private float lifetimeSeconds;

        [SerializeField]
        private ItemPassDirection passDirection;

        [SerializeField]
        private string effectKey;

        [SerializeField]
        private int effectValue;

        [SerializeField]
        private float effectDurationSeconds;

        [SerializeField]
        private bool affectsScore;

        [SerializeField]
        private bool affectsProgression;

        [SerializeField]
        private bool serverValidated;

        [SerializeField]
        private int maxAcquirePerRun;

        [SerializeField]
        private string rarity;

        public string ItemId => itemId;
        public string ServerItemId => serverItemId;
        public string TableVersion => tableVersion;
        public bool Enabled => enabled;
        public string DisplayName => displayName;
        public ItemType ItemType => itemType;
        public string IconKey => iconKey;
        public string PrefabKey => iconKey;
        public int MinFloor => minFloor;
        public int MaxFloor => maxFloor;
        public int SpawnWeight => spawnWeight;
        public int RequiredPassCount => requiredPassCount;
        public float LifetimeSeconds => lifetimeSeconds;
        public ItemPassDirection PassDirection => passDirection;
        public string EffectKey => effectKey;
        public int EffectValue => effectValue;
        public float EffectDurationSeconds => effectDurationSeconds;
        public bool AffectsScore => affectsScore;
        public bool AffectsProgression => affectsProgression;
        public bool ServerValidated => serverValidated;
        public int MaxAcquirePerRun => maxAcquirePerRun;
        public string Rarity => rarity;

        public bool CanSpawnAtFloor(int absoluteFloor)
        {
            if (!enabled || spawnWeight <= 0)
            {
                return false;
            }

            if (absoluteFloor < minFloor)
            {
                return false;
            }

            return maxFloor <= 0 || absoluteFloor <= maxFloor;
        }

        public static ItemDefinition Create(
            string itemId,
            string serverItemId,
            string tableVersion,
            bool enabled,
            string displayName,
            ItemType itemType,
            string iconKey,
            int minFloor,
            int maxFloor,
            int spawnWeight,
            int requiredPassCount,
            float lifetimeSeconds,
            ItemPassDirection passDirection,
            string effectKey,
            int effectValue,
            float effectDurationSeconds,
            bool affectsScore,
            bool affectsProgression,
            bool serverValidated,
            int maxAcquirePerRun,
            string rarity)
        {
            return new ItemDefinition
            {
                itemId = itemId,
                serverItemId = serverItemId,
                tableVersion = tableVersion,
                enabled = enabled,
                displayName = displayName,
                itemType = itemType,
                iconKey = iconKey,
                minFloor = Mathf.Max(1, minFloor),
                maxFloor = Mathf.Max(0, maxFloor),
                spawnWeight = Mathf.Max(0, spawnWeight),
                requiredPassCount = Mathf.Max(1, requiredPassCount),
                lifetimeSeconds = Mathf.Max(0f, lifetimeSeconds),
                passDirection = passDirection,
                effectKey = effectKey,
                effectValue = effectValue,
                effectDurationSeconds = Mathf.Max(0f, effectDurationSeconds),
                affectsScore = affectsScore,
                affectsProgression = affectsProgression,
                serverValidated = serverValidated,
                maxAcquirePerRun = Mathf.Max(0, maxAcquirePerRun),
                rarity = rarity
            };
        }
    }
}
