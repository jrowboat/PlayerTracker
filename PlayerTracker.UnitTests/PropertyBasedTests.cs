using Moq;
using PlayerTracker.Contracts;
using PlayerTracker.Services.Validators;
using PlayerTracker.Services;
using System.Net;
using Xunit;
using Moq.Protected;
using PlayerTracker.Interfaces;
using FluentValidation.Results;

namespace PlayerTracker.UnitTests
{
    public abstract class AbstractPropertyBasedTests
    {
        public abstract ITestApi SutFactory();


        [Fact]
        public async Task GetLeaderboards_InSeason()
        {
            // Arrange
            var sut = SutFactory();
            sut.InitializeTestData(TestDataProperty.InSeason);

            // Act
            var result = await sut.GetLeaderboards();

            // Assert
            Assert.NotEmpty(result);
            Assert.True(!result.Any(leaderboard => !leaderboard.Players.Any(player => player.Name == "Scottie Scheffler")));
        }

        [Fact]
        public async Task GetLeaderboards_SeasonHasNotStarted()
        {
            // Arrange
            var sut = SutFactory();
            sut.InitializeTestData(TestDataProperty.SeasonHasNotStarted);

            // Act
            var result = await sut.GetLeaderboards();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLeaderboards_DatesShouldNotBeInFuture()
        {
            var now = DateTime.Now;
            // Arrange
            var sut = SutFactory();
            sut.InitializeTestData(TestDataProperty.DatesShouldNotBeInFuture);

            // Act
            var result = await sut.GetLeaderboards();

            // Assert
            Assert.False(result.Where(leaderboard => leaderboard.Tournament.StartDate > now).Any(), "Tournament date found in future");
        }

        [Fact]
        public async Task GetLeaderboards_BadTournamentResults()
        {
            // Arrange
            var sut = SutFactory();
            sut.InitializeTestData(TestDataProperty.BadTournamentResults);

            // Act
            var result = await sut.GetLeaderboards();

            // Assert
            Assert.Equal(0, result.Count());
        }

        public interface ITestApi
        {
            void InitializeTestData(TestDataProperty property);
            // tournamentId assumes a database with a tournament table
            Task<IEnumerable<Leaderboard>?> GetLeaderboards();
        }
    }

    public enum TestDataProperty
    {
        InSeason,
        SeasonHasNotStarted,
        DatesShouldNotBeInFuture,
        BadTournamentResults
    }

    public class PropertyBasedTests : AbstractPropertyBasedTests
    {
        public override ITestApi SutFactory()
        {
            return new PlayerTrackerPropertyTestingApi();
        }

        public class PlayerTrackerPropertyTestingApi : ITestApi
        {
            PlayerTrackerService playerTrackerService;

            public void InitializeTestData(TestDataProperty property)
            {
                var pastDate = DateTime.Now.AddMonths(-1);
                var futureDate = DateTime.Now.AddMonths(1);

                var mockLeaderboardService = new Mock<ILeaderboardService>();
                var mockTournamentService = new Mock<ITournamentService>();

                switch (property)
                {
                    case TestDataProperty.SeasonHasNotStarted:
                        mockLeaderboardService
                            .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                            .ReturnsAsync((Leaderboard?)null);

                        mockTournamentService
                            .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                            .ReturnsAsync(Enumerable.Empty<Tournament>());
                        break;
                    case TestDataProperty.InSeason:
                        mockLeaderboardService
                            .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                            .ReturnsAsync(new Leaderboard
                            {
                                Tournament = new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = pastDate },
                                Players = new List<PlayerResult> { new PlayerResult { Name = "Scottie Scheffler", Rank = 1, TotalScore = 0 } }
                            });

                        mockTournamentService
                            .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                            .ReturnsAsync(new List<Tournament>
                            {
                                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = pastDate }
                            });
                        break;
                    case TestDataProperty.DatesShouldNotBeInFuture:
                        mockLeaderboardService
                            .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                            .ReturnsAsync(new Leaderboard
                            {
                                Tournament = new Tournament { TournamentID = 2, Name = "Tournament 2", StartDate = futureDate },
                                Players = new List<PlayerResult> { new PlayerResult { Name = "Scottie Scheffler", Rank = 1, TotalScore = 0 } }
                            });

                        mockTournamentService
                            .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                            .ReturnsAsync(new List<Tournament>
                            {
                                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = pastDate },
                                new Tournament { TournamentID = 2, Name = "Tournament 2", StartDate = futureDate }
                            });
                        break;
                    case TestDataProperty.BadTournamentResults:
                        mockLeaderboardService
                            .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                            .ReturnsAsync(new Leaderboard
                            {
                                Tournament = new Tournament { TournamentID = 1, StartDate = pastDate },
                                Players = new List<PlayerResult> { new PlayerResult { Name = "Scottie Scheffler" } }
                            });

                        mockTournamentService
                            .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                            .ReturnsAsync(new List<Tournament>
                            {
                                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = pastDate },
                                new Tournament { TournamentID = 2, Name = "Tournament 2" }
                            });
                        break;
                }

                playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, new TournamentValidator(), new LeaderboardValidator());
            }

            public async Task<IEnumerable<Leaderboard>?> GetLeaderboards()
            {
                return await playerTrackerService.GetLeaderboards();
            }
        }
    }
}
