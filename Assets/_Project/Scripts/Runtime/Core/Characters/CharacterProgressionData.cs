using System;
using System.Collections.Generic;

namespace LootUp.Core.Characters
{
    [Serializable]
    public sealed class CharacterProgressionData
    {
        public string CharacterId;
        public int Level = 1;
        public int CurrentExperience;
        public bool IsOwned;
        public bool IsEquipped;
    }

    [Serializable]
    public sealed class CharacterProgressionSaveData
    {
        public int Version = 2;
        public string SelectedCharacterId;
        public string EquippedCharacterId;
        public List<CharacterProgressionData> Characters = new List<CharacterProgressionData>();
    }

    public readonly struct CharacterProgressionRecord
    {
        public CharacterProgressionRecord(string characterId, int level, int currentExperience, bool isOwned, bool isEquipped)
        {
            CharacterId = characterId ?? string.Empty;
            Level = Math.Max(1, level);
            CurrentExperience = Math.Max(0, currentExperience);
            IsOwned = isOwned;
            IsEquipped = isEquipped;
        }

        public string CharacterId { get; }
        public int Level { get; }
        public int CurrentExperience { get; }
        public bool IsOwned { get; }
        public bool IsEquipped { get; }
    }
}
