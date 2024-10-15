using PlayerTracker.Contracts;
using PlayerTracker.Interfaces;
using PlayerTracker.Services.Validators;

namespace PlayerTracker.Services
{
    public class PlayerTrackerService : IPlayerTrackerService
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly ITournamentService _tournamentService;
        private readonly TournamentValidator _tournamentValidator;
        private readonly LeaderboardValidator _leaderboardValidator;

        public PlayerTrackerService(ILeaderboardService leaderboardService, ITournamentService tournamentService, TournamentValidator tournamentValidator, LeaderboardValidator leaderboardValidator)
        {
            _leaderboardService = leaderboardService;
            _tournamentService = tournamentService;
            _tournamentValidator = tournamentValidator;
            _leaderboardValidator = leaderboardValidator;
        }

        public async Task<IEnumerable<Leaderboard>?> GetLeaderboards()
        {
            int currentYear = DateTime.Now.Year;
            var tournaments = await _tournamentService.GetMostRecentTournaments(currentYear);

            if (tournaments == null)
                return Enumerable.Empty<Leaderboard>();

            var playedLeaderboards = await GetPlayedLeaderboards(tournaments);

            return playedLeaderboards;
        }

        private async Task<IEnumerable<Leaderboard>> GetPlayedLeaderboards(IEnumerable<Tournament> tournaments)
        {
            var validTournaments = await ValidateTournaments(tournaments);
            var allLeaderboards = await GetAllLeaderboards(validTournaments);
            var playedLeaderboards = FilterPlayedLeaderboards(allLeaderboards);

            return playedLeaderboards;
        }

        private async Task<IEnumerable<Tournament>> ValidateTournaments(IEnumerable<Tournament> tournaments)
        {
            var validTournaments = new List<Tournament>();

            foreach (var tournament in tournaments)
            {
                var validationResult = _tournamentValidator.Validate(tournament);

                if (!validationResult.IsValid)
                {
                    Console.WriteLine($"Validation failed for tournament {tournament.Name}:");
                }
                else
                {
                    validTournaments.Add(tournament);
                }
            }

            return validTournaments;
        }

        private async Task<IEnumerable<Leaderboard>> GetAllLeaderboards(IEnumerable<Tournament> tournaments)
        {
            var leaderboardTasks = tournaments
                .Select(tournament => _leaderboardService.GetLeaderboard(tournament.TournamentID.ToString()))
                .Where(leaderboard => leaderboard is not null);

            var allLeaderboards = await Task.WhenAll(leaderboardTasks);

            return allLeaderboards;
        }

        private IEnumerable<Leaderboard> FilterPlayedLeaderboards(IEnumerable<Leaderboard> leaderboards)
        {
            var playedLeaderboards = leaderboards
                .Where(leaderboard => leaderboard.Players != Enumerable.Empty<PlayerResult>() 
                    && leaderboard.Players.Any(player => player.Name == "Scottie Scheffler"));

            var validLeaderboards = new List<Leaderboard>();

            foreach (var leaderboard in playedLeaderboards)
            {
                var validationResult = _leaderboardValidator.Validate(leaderboard);

                if (!validationResult.IsValid)
                {
                    Console.WriteLine($"Validation failed for leaderboard {leaderboard.Tournament.Name}:");
                }
                else
                {
                    validLeaderboards.Add(leaderboard);
                }
            }

            return validLeaderboards;
        }
    }
}
