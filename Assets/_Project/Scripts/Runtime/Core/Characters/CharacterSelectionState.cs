using System;

namespace PH.Core.Characters
{
    public static class CharacterSelectionState
    {
        private static CharacterDefinition selectedCharacter;

        public static CharacterDefinition SelectedCharacter => selectedCharacter;

        public static bool HasSelection => selectedCharacter != null;

        public static string SelectedCharacterId => selectedCharacter != null ? selectedCharacter.CharacterId : string.Empty;

        public static void Select(CharacterDefinition characterDefinition)
        {
            if (characterDefinition == null || !CharacterProgressionState.TrySelectAndEquip(characterDefinition))
            {
                return;
            }

            selectedCharacter = characterDefinition;
        }

        public static CharacterDefinition Resolve(CharacterDefinition fallbackCharacter)
        {
            if (selectedCharacter != null && CharacterProgressionState.IsOwned(selectedCharacter))
            {
                return selectedCharacter;
            }

            return fallbackCharacter != null && CharacterProgressionState.IsOwned(fallbackCharacter)
                ? fallbackCharacter
                : null;
        }

        public static CharacterDefinition Resolve(CharacterDefinition fallbackCharacter, CharacterDefinition[] availableCharacters)
        {
            if (selectedCharacter != null && CharacterProgressionState.IsOwned(selectedCharacter))
            {
                return selectedCharacter;
            }

            string savedCharacterId = !string.IsNullOrWhiteSpace(CharacterProgressionState.EquippedCharacterId)
                ? CharacterProgressionState.EquippedCharacterId
                : CharacterProgressionState.SelectedCharacterId;
            CharacterDefinition savedCharacter = FindById(availableCharacters, savedCharacterId);
            if (savedCharacter != null && CharacterProgressionState.IsOwned(savedCharacter))
            {
                selectedCharacter = savedCharacter;
                return selectedCharacter;
            }

            if (fallbackCharacter != null && CharacterProgressionState.IsOwned(fallbackCharacter))
            {
                selectedCharacter = fallbackCharacter;
                return selectedCharacter;
            }

            if (availableCharacters == null)
            {
                return null;
            }

            for (int i = 0; i < availableCharacters.Length; i++)
            {
                CharacterDefinition candidate = availableCharacters[i];
                if (candidate != null && CharacterProgressionState.IsOwned(candidate))
                {
                    selectedCharacter = candidate;
                    return selectedCharacter;
                }
            }

            return null;
        }

        private static CharacterDefinition FindById(CharacterDefinition[] availableCharacters, string characterId)
        {
            if (availableCharacters == null || string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            for (int i = 0; i < availableCharacters.Length; i++)
            {
                CharacterDefinition candidate = availableCharacters[i];
                if (candidate != null && string.Equals(candidate.CharacterId, characterId, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
