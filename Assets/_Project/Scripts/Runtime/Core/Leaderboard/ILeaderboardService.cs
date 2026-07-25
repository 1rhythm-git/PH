using System.Threading.Tasks;

namespace LootUp.Core.Leaderboard
{
    public interface ILeaderboardService
    {
        bool IsOnline { get; }

        Task<LeaderboardSnapshot> LoadAsync(int limit);

        Task<LeaderboardSubmitResult> SubmitAsync(
            LeaderboardRecord record);
    }
}
