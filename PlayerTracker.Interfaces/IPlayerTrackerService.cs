using PlayerTracker.Contracts;

namespace PlayerTracker.Interfaces
{
    public interface IPlayerTrackerService
    {
        Task<IEnumerable<Leaderboard>?> GetLeaderboards();
    }
}