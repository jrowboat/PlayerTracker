using Microsoft.Extensions.DependencyInjection;
using PlayerTracker.Contracts;
using PlayerTracker.Interfaces;
using PlayerTracker.Services;
using PlayerTracker.Services.Validators;

namespace PlayerTracker.App
{
    public class Program
    {
        private static IServiceProvider serviceProvider;

        private readonly IPlayerTrackerService  _playerTrackerService;

        public Program(IPlayerTrackerService playerTrackerService)
        {
            _playerTrackerService = playerTrackerService;
        }

        public static void Main(string[] args)
        {
            // Set up dependency injection
            var services = new ServiceCollection();
            services.AddHttpClient<ILeaderboardService, LeaderboardService>();
            services.AddHttpClient<ITournamentService, TournamentService>();
            services.AddTransient<IPlayerTrackerService, PlayerTrackerService>();
            services.AddTransient<LeaderboardValidator>();
            services.AddTransient<TournamentValidator>();

            services.AddTransient<Program>();
            serviceProvider = services.BuildServiceProvider();

            // Run the program
            serviceProvider.GetService<Program>().Run(args).GetAwaiter().GetResult();
        }

        public async Task Run(string[] args)
        {
            var leaderboards = await _playerTrackerService.GetLeaderboards();
            if (leaderboards != null)
            {
                foreach (var leaderboard in leaderboards)
                {
                    PrintPlayerResults(leaderboard.Tournament.Name, leaderboard.Players.First(player => player.Name == "Scottie Scheffler"));
                }
            }
        }

        // Method to print the leaderboard
        static void PrintPlayerResults(string tournamentName, PlayerResult player)
        {
            Console.WriteLine($"Leaderboard for Tournament: {tournamentName}");
            Console.WriteLine();

            Console.WriteLine($"Position: {player.Rank}");
            Console.WriteLine($"Player: {player.Name} ");
            Console.WriteLine($"Total Score: {player.TotalScore}");
            Console.WriteLine();
        }
    }
}
