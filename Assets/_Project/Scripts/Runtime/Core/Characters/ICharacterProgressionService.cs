namespace LootUp.Core.Characters
{
    public interface ICharacterProgressionService
    {
        string SelectedCharacterId { get; }
        string EquippedCharacterId { get; }
        CharacterProgressionRecord GetOrCreate(string characterId, bool initiallyOwned);
        bool SetProgress(string characterId, int level, int currentExperience, bool initiallyOwned);
        bool SetOwned(string characterId, bool isOwned);
        bool SetSelectedAndEquipped(string characterId);
        bool TrySave();
    }
}
