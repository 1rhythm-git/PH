using System;
using System.Collections.Generic;
using UnityEngine;

namespace PH.Core.Characters
{
    public sealed class LocalCharacterProgressionService : ICharacterProgressionService
    {
        private const int CurrentVersion = 2;
        private const string SaveKey = "PH.CharacterProgression.v1";

        private readonly CharacterProgressionSaveData saveData;

        public LocalCharacterProgressionService()
        {
            saveData = Load();
            NormalizeSaveData();
        }

        public string SelectedCharacterId => saveData.SelectedCharacterId ?? string.Empty;
        public string EquippedCharacterId => saveData.EquippedCharacterId ?? string.Empty;

        public CharacterProgressionRecord GetOrCreate(string characterId, bool initiallyOwned)
        {
            string normalizedId = NormalizeId(characterId);
            if (string.IsNullOrEmpty(normalizedId))
            {
                return new CharacterProgressionRecord(string.Empty, 1, 0, false, false);
            }

            CharacterProgressionData entry = FindCharacter(normalizedId);
            if (entry == null)
            {
                entry = new CharacterProgressionData
                {
                    CharacterId = normalizedId,
                    IsOwned = initiallyOwned
                };
                saveData.Characters.Add(entry);
                TrySave();
            }

            return CreateRecord(entry);
        }

        public bool SetProgress(string characterId, int level, int currentExperience, bool initiallyOwned)
        {
            string normalizedId = NormalizeId(characterId);
            if (string.IsNullOrEmpty(normalizedId))
            {
                return false;
            }

            CharacterProgressionData entry = FindOrCreateCharacter(normalizedId, initiallyOwned);
            entry.Level = Mathf.Max(1, level);
            entry.CurrentExperience = Mathf.Max(0, currentExperience);
            return TrySave();
        }

        public bool SetOwned(string characterId, bool isOwned)
        {
            string normalizedId = NormalizeId(characterId);
            if (string.IsNullOrEmpty(normalizedId))
            {
                return false;
            }

            CharacterProgressionData entry = FindOrCreateCharacter(normalizedId, false);
            entry.IsOwned = isOwned;
            if (!isOwned)
            {
                entry.IsEquipped = false;
                ClearSelection(normalizedId);
            }

            return TrySave();
        }

        public bool SetSelectedAndEquipped(string characterId)
        {
            string normalizedId = NormalizeId(characterId);
            CharacterProgressionData selectedEntry = FindCharacter(normalizedId);
            if (selectedEntry == null || !selectedEntry.IsOwned)
            {
                return false;
            }

            for (int i = 0; i < saveData.Characters.Count; i++)
            {
                CharacterProgressionData entry = saveData.Characters[i];
                if (entry != null)
                {
                    entry.IsEquipped = string.Equals(entry.CharacterId, normalizedId, StringComparison.Ordinal);
                }
            }

            saveData.SelectedCharacterId = normalizedId;
            saveData.EquippedCharacterId = normalizedId;
            return TrySave();
        }

        public bool TrySave()
        {
            try
            {
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Character progression save failed: {exception.Message}");
                return false;
            }
        }

        private CharacterProgressionSaveData Load()
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new CharacterProgressionSaveData();
            }

            try
            {
                return JsonUtility.FromJson<CharacterProgressionSaveData>(json) ?? new CharacterProgressionSaveData();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Character progression load failed: {exception.Message}");
                return new CharacterProgressionSaveData();
            }
        }

        private void NormalizeSaveData()
        {
            saveData.Characters ??= new List<CharacterProgressionData>();
            saveData.Version = CurrentVersion;
            saveData.SelectedCharacterId = NormalizeId(saveData.SelectedCharacterId);
            saveData.EquippedCharacterId = NormalizeId(saveData.EquippedCharacterId);

            Dictionary<string, CharacterProgressionData> knownEntries = new Dictionary<string, CharacterProgressionData>(StringComparer.Ordinal);
            for (int i = saveData.Characters.Count - 1; i >= 0; i--)
            {
                CharacterProgressionData entry = saveData.Characters[i];
                if (entry == null)
                {
                    saveData.Characters.RemoveAt(i);
                    continue;
                }

                entry.CharacterId = NormalizeId(entry.CharacterId);
                if (string.IsNullOrEmpty(entry.CharacterId))
                {
                    saveData.Characters.RemoveAt(i);
                    continue;
                }

                entry.Level = Mathf.Max(1, entry.Level);
                entry.CurrentExperience = Mathf.Max(0, entry.CurrentExperience);
                if (knownEntries.TryGetValue(entry.CharacterId, out CharacterProgressionData existingEntry))
                {
                    MergeProgress(existingEntry, entry);
                    saveData.Characters.RemoveAt(i);
                    continue;
                }

                knownEntries.Add(entry.CharacterId, entry);
            }

            CharacterProgressionData equippedEntry = FindCharacter(saveData.EquippedCharacterId);
            if (equippedEntry == null || !equippedEntry.IsOwned)
            {
                saveData.EquippedCharacterId = string.Empty;
            }

            CharacterProgressionData selectedEntry = FindCharacter(saveData.SelectedCharacterId);
            if (selectedEntry == null || !selectedEntry.IsOwned)
            {
                saveData.SelectedCharacterId = saveData.EquippedCharacterId;
            }

            for (int i = 0; i < saveData.Characters.Count; i++)
            {
                CharacterProgressionData entry = saveData.Characters[i];
                entry.IsEquipped = string.Equals(entry.CharacterId, saveData.EquippedCharacterId, StringComparison.Ordinal)
                    && entry.IsOwned;
            }

            TrySave();
        }

        private CharacterProgressionData FindOrCreateCharacter(string characterId, bool initiallyOwned)
        {
            CharacterProgressionData entry = FindCharacter(characterId);
            if (entry != null)
            {
                return entry;
            }

            entry = new CharacterProgressionData
            {
                CharacterId = characterId,
                IsOwned = initiallyOwned
            };
            saveData.Characters.Add(entry);
            return entry;
        }

        private CharacterProgressionData FindCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                return null;
            }

            for (int i = 0; i < saveData.Characters.Count; i++)
            {
                CharacterProgressionData entry = saveData.Characters[i];
                if (entry != null && string.Equals(entry.CharacterId, characterId, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        private void ClearSelection(string characterId)
        {
            if (string.Equals(saveData.SelectedCharacterId, characterId, StringComparison.Ordinal))
            {
                saveData.SelectedCharacterId = string.Empty;
            }

            if (string.Equals(saveData.EquippedCharacterId, characterId, StringComparison.Ordinal))
            {
                saveData.EquippedCharacterId = string.Empty;
            }
        }

        private CharacterProgressionRecord CreateRecord(CharacterProgressionData entry)
        {
            return new CharacterProgressionRecord(
                entry.CharacterId,
                entry.Level,
                entry.CurrentExperience,
                entry.IsOwned,
                entry.IsEquipped);
        }

        private void MergeProgress(CharacterProgressionData target, CharacterProgressionData source)
        {
            if (source.Level > target.Level)
            {
                target.Level = source.Level;
                target.CurrentExperience = source.CurrentExperience;
            }
            else if (source.Level == target.Level)
            {
                target.CurrentExperience = Mathf.Max(target.CurrentExperience, source.CurrentExperience);
            }

            target.IsOwned |= source.IsOwned;
        }

        private string NormalizeId(string characterId)
        {
            return CharacterIdMigration.Normalize(characterId);
        }
    }
}
