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
        private float remainingSeconds;

        [SerializeField]
        private int remainingHearts;

        [SerializeField]
        private List<ItemRunEvent> acquiredItemEvents = new List<ItemRunEvent>();

        public GameOverReason GameOverReason => gameOverReason;
        public int HighestFloor => highestFloor;
        public int Score => score;
        public float RemainingSeconds => remainingSeconds;
        public int RemainingHearts => remainingHearts;
        public IReadOnlyList<ItemRunEvent> AcquiredItemEvents => acquiredItemEvents;

        public RunResultData(
            GameOverReason gameOverReason,
            int highestFloor,
            int score,
            float remainingSeconds,
            int remainingHearts,
            IReadOnlyList<ItemRunEvent> acquiredItemEvents)
        {
            this.gameOverReason = gameOverReason;
            this.highestFloor = Mathf.Max(1, highestFloor);
            this.score = Mathf.Max(0, score);
            this.remainingSeconds = Mathf.Max(0f, remainingSeconds);
            this.remainingHearts = Mathf.Max(0, remainingHearts);
            this.acquiredItemEvents = acquiredItemEvents == null ? new List<ItemRunEvent>() : new List<ItemRunEvent>(acquiredItemEvents);
        }
    }
}
