using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LootUp.Core.Game;
using LootUp.Core.Profile;
using UnityEngine;

namespace LootUp.Core.Leaderboard
{
    public static class LeaderboardManager
    {
        private static ILeaderboardService service;
        private static int serviceGeneration;
        private static bool isSubmitting;

        public static event Action SnapshotChanged;

        public static LeaderboardSnapshot Snapshot { get; private set; } =
            CreateLocalSnapshot(LeaderboardLoadState.Local, string.Empty);

        public static void Configure(ILeaderboardService leaderboardService)
        {
            service = leaderboardService;
            serviceGeneration++;
            isSubmitting = false;
            Snapshot = leaderboardService != null
                       && leaderboardService.IsOnline
                ? new LeaderboardSnapshot(
                    LeaderboardLoadState.Loading,
                    Array.Empty<LeaderboardRecord>(),
                    null,
                    string.Empty)
                : CreateLocalSnapshot(
                    LeaderboardLoadState.Local,
                    string.Empty);
            SnapshotChanged?.Invoke();
        }

        public static async Task RefreshAsync(int limit = 10)
        {
            ILeaderboardService activeService = service;
            int generation = serviceGeneration;
            if (activeService == null || !activeService.IsOnline)
            {
                Snapshot = CreateLocalSnapshot(
                    LeaderboardLoadState.Local,
                    string.Empty);
                SnapshotChanged?.Invoke();
                return;
            }

            Snapshot = new LeaderboardSnapshot(
                LeaderboardLoadState.Loading,
                Snapshot.Records,
                Snapshot.MyRecord,
                string.Empty);
            SnapshotChanged?.Invoke();

            try
            {
                LeaderboardSnapshot loaded =
                    await activeService.LoadAsync(Math.Max(1, limit));
                if (generation != serviceGeneration)
                {
                    return;
                }

                Snapshot = loaded;
            }
            catch (Exception exception)
            {
                if (generation != serviceGeneration)
                {
                    return;
                }

                Snapshot = new LeaderboardSnapshot(
                    LeaderboardLoadState.Error,
                    Array.Empty<LeaderboardRecord>(),
                    null,
                    exception.Message);
            }

            SnapshotChanged?.Invoke();
        }

        public static async Task SubmitRunAsync(RunResultData resultData)
        {
            ILeaderboardService activeService = service;
            if (resultData == null
                || activeService == null
                || !activeService.IsOnline
                || isSubmitting)
            {
                return;
            }

            isSubmitting = true;
            try
            {
                LeaderboardRecord record = new LeaderboardRecord(
                    0,
                    UserProfileManager.UserId,
                    UserProfileManager.Nickname,
                    resultData.HighestFloor,
                    resultData.Score,
                    resultData.CharacterId,
                    resultData.CharacterLevel);
                LeaderboardSubmitResult result =
                    await activeService.SubmitAsync(record);
                if (!result.Succeeded)
                {
                    Debug.LogWarning(
                        $"Leaderboard submission failed: {result.Message}");
                    return;
                }

                if (result.Updated)
                {
                    await RefreshAsync();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Leaderboard submission failed: {exception.Message}");
            }
            finally
            {
                isSubmitting = false;
            }
        }

        private static LeaderboardSnapshot CreateLocalSnapshot(
            LeaderboardLoadState state,
            string message)
        {
            bool hasRecord = UserProfileManager.BestHighestFloor > 0
                             || UserProfileManager.BestScore > 0;
            LeaderboardRecord localRecord = hasRecord
                ? new LeaderboardRecord(
                    1,
                    UserProfileManager.UserId,
                    UserProfileManager.Nickname,
                    UserProfileManager.BestHighestFloor,
                    UserProfileManager.BestScore,
                    UserProfileManager.BestCharacterId,
                    UserProfileManager.BestCharacterLevel)
                : null;
            IReadOnlyList<LeaderboardRecord> records = localRecord != null
                ? new[] { localRecord }
                : Array.Empty<LeaderboardRecord>();
            return new LeaderboardSnapshot(
                state,
                records,
                localRecord,
                message);
        }
    }
}
