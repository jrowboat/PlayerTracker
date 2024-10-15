using Newtonsoft.Json;
using PlayerTracker.Contracts;
using PlayerTracker.Interfaces;
using PlayerTracker.Services.Validators;

namespace PlayerTracker.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly HttpClient _httpClient;

        public LeaderboardService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Method to get the leaderboard for a specific tournament
        public async Task<Leaderboard?> GetLeaderboard(string tournamentId)
        {
            string apiKey = "c0f48ac64c714d4f966fd7069e880ddb";
            string baseUrl = "https://api.sportsdata.io/golf/v2/json";
            string leaderboardUrl = $"{baseUrl}/Leaderboard/{tournamentId}?key={apiKey}";

            var response = await _httpClient.GetAsync(leaderboardUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var leaderboard = JsonConvert.DeserializeObject<Leaderboard>(content);

                return leaderboard;
            }
            else
            {
                // Handle error response
                Console.WriteLine($"Failed to fetch data. Status code: {response.StatusCode}");
                return null;
            }
        }
    }
}
