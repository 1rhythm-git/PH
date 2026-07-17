using System;
using System.Collections.Generic;
using UnityEngine;

namespace PH.Core.Characters
{
    public readonly struct CharacterProgressionSnapshot
    {
        public CharacterProgressionSnapshot(string characterId, int level, int currentExperience, int requiredExperience)
        {
            CharacterId = characterId;
            Level = Mathf.Max(1, level);
            CurrentExperience = Mathf.Max(0, currentExperience);
            RequiredExperience = Mathf.Max(0, requiredExperience);
        }

        public string CharacterId { get; }
        public int Level { get; }
        public int CurrentExperience { get; }
        public int RequiredExperience { get; }
        public bool IsMaxLevel => RequiredExperience <= 0;
        public float NormalizedExperience => IsMaxLevel
            ? 1f
            : Mathf.Clamp01((float)CurrentExperience / RequiredExperience);
    }

    public static class CharacterProgressionState
    {
        private static readonly Dictionary<string, ProgressEntry> ProgressByCharacterId = new Dictionary<string, ProgressEntry>();

        public static event Action<string> ProgressChanged;

        public static CharacterProgressionSnapshot GetSnapshot(CharacterDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.CharacterId))
            {
                return new CharacterProgressionSnapshot(string.Empty, 1, 0, 0);
            }

            ProgressEntry entry = GetOrCreateEntry(definition);
            int requiredExperience = definition.GetRequiredExperienceForLevel(entry.Level);
            return new CharacterProgressionSnapshot(definition.CharacterId, entry.Level, entry.CurrentExperience, requiredExperience);
        }

        // (추가) 결과식이 확정되면 런 결과 계층에서 호출할 캐릭터 XP 지급 진입점이다.
        public static CharacterProgressionSnapshot AddExperience(CharacterDefinition definition, int amount)
        {
            if (definition == null || amount <= 0 || string.IsNullOrWhiteSpace(definition.CharacterId))
            {
                return GetSnapshot(definition);
            }

            ProgressEntry entry = GetOrCreateEntry(definition);
            entry.CurrentExperience += Mathf.Max(0, amount);

            while (entry.Level < definition.MaxCharacterLevel)
            {
                int requiredExperience = definition.GetRequiredExperienceForLevel(entry.Level);
                if (requiredExperience <= 0 || entry.CurrentExperience < requiredExperience)
                {
                    break;
                }

                entry.CurrentExperience -= requiredExperience;
                entry.Level++;
            }

            if (entry.Level >= definition.MaxCharacterLevel)
            {
                entry.Level = definition.MaxCharacterLevel;
                entry.CurrentExperience = 0;
            }

            ProgressChanged?.Invoke(definition.CharacterId);
            return GetSnapshot(definition);
        }

        // (추가) 추후 로컬 또는 BackND 저장 데이터를 주입할 수 있는 복원 API다.
        public static void SetProgress(CharacterDefinition definition, int level, int currentExperience)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.CharacterId))
            {
                return;
            }

            ProgressEntry entry = GetOrCreateEntry(definition);
            entry.Level = Mathf.Clamp(level, 1, definition.MaxCharacterLevel);
            entry.CurrentExperience = Mathf.Max(0, currentExperience);
            NormalizeProgress(definition, entry);
            ProgressChanged?.Invoke(definition.CharacterId);
        }

        public static bool IsSkillUnlocked(CharacterDefinition definition)
        {
            return definition != null && GetSnapshot(definition).Level >= definition.SkillUnlockLevel;
        }

        public static float GetActiveSkillItemPageSpawnChance(CharacterDefinition definition)
        {
            return IsSkillUnlocked(definition) ? definition.SkillItemPageSpawnChance : 0f;
        }

        private static ProgressEntry GetOrCreateEntry(CharacterDefinition definition)
        {
            if (!ProgressByCharacterId.TryGetValue(definition.CharacterId, out ProgressEntry entry))
            {
                entry = new ProgressEntry();
                ProgressByCharacterId.Add(definition.CharacterId, entry);
            }

            NormalizeProgress(definition, entry);
            return entry;
        }

        private static void NormalizeProgress(CharacterDefinition definition, ProgressEntry entry)
        {
            entry.Level = Mathf.Clamp(entry.Level, 1, definition.MaxCharacterLevel);
            entry.CurrentExperience = Mathf.Max(0, entry.CurrentExperience);

            while (entry.Level < definition.MaxCharacterLevel)
            {
                int requiredExperience = definition.GetRequiredExperienceForLevel(entry.Level);
                if (requiredExperience <= 0 || entry.CurrentExperience < requiredExperience)
                {
                    break;
                }

                entry.CurrentExperience -= requiredExperience;
                entry.Level++;
            }

            if (entry.Level >= definition.MaxCharacterLevel)
            {
                entry.CurrentExperience = 0;
            }
        }

        private sealed class ProgressEntry
        {
            public int Level = 1;
            public int CurrentExperience;
        }
    }
}
