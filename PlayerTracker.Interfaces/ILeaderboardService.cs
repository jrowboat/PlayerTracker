using PlayerTracker.Contracts;
using System.Threading.Tasks;

namespace PlayerTracker.Interfaces
{
    public interface ILeaderboardService
    {
        Task<Leaderboard?> GetLeaderboard(string tournamentId);
    }
}
