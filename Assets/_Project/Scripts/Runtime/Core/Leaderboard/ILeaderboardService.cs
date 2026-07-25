using System.Threading.Tasks;

namespace LootUp.Core.Leaderboard
{
    public interface ILeaderboardService
    {
        bool IsOnline { get; }

        Task<LeaderboardSnapshot> LoadAsync(int limit);

        Task<LeaderboardRecord> SynchronizeLifetimeBestAsync(
            LeaderboardRecord localRecord);

        Task<LeaderboardSubmitResult> SubmitAsync(
            LeaderboardRecord record);
    }
}
