using System;
using System.Collections.Generic;

namespace LootUp.Core.Leaderboard
{
    public enum LeaderboardLoadState
    {
        Local,
        Loading,
        Online,
        Error
    }

    public sealed class LeaderboardRecord
    {
        public LeaderboardRecord(
            int rank,
            string userId,
            string nickname,
            int highestFloor,
            int score,
            string characterId,
            int characterLevel)
        {
            Rank = Math.Max(0, rank);
            UserId = userId ?? string.Empty;
            Nickname = string.IsNullOrWhiteSpace(nickname)
                ? "Player"
                : nickname.Trim();
            HighestFloor = Math.Max(0, highestFloor);
            Score = Math.Max(0, score);
            CharacterId = characterId ?? string.Empty;
            CharacterLevel = Math.Max(1, characterLevel);
        }

        public int Rank { get; }
        public string UserId { get; }
        public string Nickname { get; }
        public int HighestFloor { get; }
        public int Score { get; }
        public string CharacterId { get; }
        public int CharacterLevel { get; }
        public bool HasRecord => HighestFloor > 0 || Score > 0;
    }

    public sealed class LeaderboardSnapshot
    {
        public LeaderboardSnapshot(
            LeaderboardLoadState state,
            IReadOnlyList<LeaderboardRecord> records,
            LeaderboardRecord myRecord,
            string message)
        {
            State = state;
            Records = records ?? Array.Empty<LeaderboardRecord>();
            MyRecord = myRecord;
            Message = message ?? string.Empty;
        }

        public LeaderboardLoadState State { get; }
        public IReadOnlyList<LeaderboardRecord> Records { get; }
        public LeaderboardRecord MyRecord { get; }
        public string Message { get; }
        public bool IsOnline => State == LeaderboardLoadState.Online;
    }

    public readonly struct LeaderboardSubmitResult
    {
        private LeaderboardSubmitResult(
            bool succeeded,
            bool updated,
            string message)
        {
            Succeeded = succeeded;
            Updated = updated;
            Message = message ?? string.Empty;
        }

        public bool Succeeded { get; }
        public bool Updated { get; }
        public string Message { get; }

        public static LeaderboardSubmitResult Success(bool updated)
        {
            return new LeaderboardSubmitResult(true, updated, string.Empty);
        }

        public static LeaderboardSubmitResult Fail(string message)
        {
            return new LeaderboardSubmitResult(false, false, message);
        }
    }

    [Serializable]
    internal sealed class LeaderboardRecordPayload
    {
        public int v = 1;
        public int f;
        public int s;
        public int l = 1;
        public string c = string.Empty;
    }
}
