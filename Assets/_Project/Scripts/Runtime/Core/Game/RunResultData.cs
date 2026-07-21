using System;
using System.Collections.Generic;
using PH.Core.Items;
using UnityEngine;

namespace PH.Core.Game
{
    [Serializable]
    public sealed class RunResultData
    {
        [SerializeField]
        private GameOverReason gameOverReason;

        [SerializeField]
        private int highestFloor;

        [SerializeField]
        private int score;

        [SerializeField]
        private int gameplayScore;

        [SerializeField]
        private int floorScore;

        [SerializeField]
        private int lifeScore;

        [SerializeField]
        private string characterId;

        [SerializeField]
        private int characterLevel;

        [SerializeField]
        private int levelExperience;

        [SerializeField]
        private int floorExperience;

        [SerializeField]
        private int scoreExperience;

        [SerializeField]
        private int totalExperience;

        [SerializeField]
        private int acquiredGameMoney;

        [SerializeField]
        private int bonusGameMoney;

        [SerializeField]
        private float remainingSeconds;

        [SerializeField]
        private int remainingHearts;

        [SerializeField]
        private List<ItemRunEvent> acquiredItemEvents = new List<ItemRunEvent>();

        public GameOverReason GameOverReason => gameOverReason;
        public int HighestFloor => highestFloor;
        public int Score => score;
        public int GameplayScore => gameplayScore;
        public int FloorScore => floorScore;
        public int LifeScore => lifeScore;
        public string CharacterId => characterId;
        public int CharacterLevel => characterLevel;
        public int LevelExperience => levelExperience;
        public int FloorExperience => floorExperience;
        public int ScoreExperience => scoreExperience;
        public int BonusExperience => scoreExperience;
        public int TotalExperience => totalExperience;
        public int AcquiredGameMoney => acquiredGameMoney;
        public int BonusGameMoney => bonusGameMoney;
        public int TotalGameMoney => acquiredGameMoney + bonusGameMoney;
        public float RemainingSeconds => remainingSeconds;
        public int RemainingHearts => remainingHearts;
        public IReadOnlyList<ItemRunEvent> AcquiredItemEvents => acquiredItemEvents;

        public RunResultData(
            GameOverReason gameOverReason,
            int highestFloor,
            RunRewardBreakdown rewards,
            string characterId,
            int characterLevel,
            float remainingSeconds,
            int remainingHearts,
            IReadOnlyList<ItemRunEvent> acquiredItemEvents)
        {
            this.gameOverReason = gameOverReason;
            this.highestFloor = Mathf.Max(1, highestFloor);
            gameplayScore = rewards.GameplayScore;
            floorScore = rewards.FloorScore;
            lifeScore = rewards.LifeScore;
            score = rewards.TotalScore;
            this.characterId = characterId ?? string.Empty;
            this.characterLevel = Mathf.Max(1, characterLevel);
            levelExperience = rewards.LevelExperience;
            floorExperience = rewards.FloorExperience;
            scoreExperience = rewards.ScoreExperience;
            totalExperience = rewards.TotalExperience;
            acquiredGameMoney = rewards.AcquiredGameMoney;
            bonusGameMoney = rewards.BonusGameMoney;
            this.remainingSeconds = Mathf.Max(0f, remainingSeconds);
            this.remainingHearts = Mathf.Max(0, remainingHearts);
            this.acquiredItemEvents = acquiredItemEvents == null ? new List<ItemRunEvent>() : new List<ItemRunEvent>(acquiredItemEvents);
        }
    }
}
