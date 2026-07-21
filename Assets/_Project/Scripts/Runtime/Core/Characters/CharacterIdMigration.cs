using System;

namespace PH.Core.Characters
{
    internal static class CharacterIdMigration
    {
        private const string LegacyNinjaId = "triangle_low_spec";
        private const string NinjaId = "ninja";

        public static string Normalize(string characterId)
        {
            string normalizedId = string.IsNullOrWhiteSpace(characterId) ? string.Empty : characterId.Trim();
            return string.Equals(normalizedId, LegacyNinjaId, StringComparison.Ordinal)
                ? NinjaId
                : normalizedId;
        }
    }
}
