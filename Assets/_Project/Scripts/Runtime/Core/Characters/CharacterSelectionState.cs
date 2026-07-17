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
            if (characterDefinition == null)
            {
                return;
            }

            selectedCharacter = characterDefinition;
        }

        public static CharacterDefinition Resolve(CharacterDefinition fallbackCharacter)
        {
            return selectedCharacter != null ? selectedCharacter : fallbackCharacter;
        }
    }
}
