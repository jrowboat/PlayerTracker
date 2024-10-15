using Newtonsoft.Json;
using PlayerTracker.Contracts;
using PlayerTracker.Interfaces;
using PlayerTracker.Services.Validators;

namespace PlayerTracker.Services
{
    public class TournamentService : ITournamentService
    {
        private readonly HttpClient _httpClient;

        public TournamentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Method to get the list of tournaments
        public async Task<IEnumerable<Tournament>?> GetMostRecentTournaments(int year)
        {
            string apiKey = "c0f48ac64c714d4f966fd7069e880ddb";
            string baseUrl = "https://api.sportsdata.io/golf/v2/json";
            string tournamentsUrl = $"{baseUrl}/Tournaments/{year}?key={apiKey}";

            var response = await _httpClient.GetAsync(tournamentsUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var tournaments = JsonConvert.DeserializeObject<Tournament[]>(json);

                return tournaments!;
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
