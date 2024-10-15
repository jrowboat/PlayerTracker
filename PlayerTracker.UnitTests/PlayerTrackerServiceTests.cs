using Moq;
using PlayerTracker.Contracts;
using PlayerTracker.Interfaces;
using PlayerTracker.Services.Validators;
using PlayerTracker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentValidation.Results;

namespace PlayerTracker.UnitTests
{
    public class PlayerTrackerServiceTests
    {
        [Fact]
        public async Task GetLeaderboards_ReturnsNonNullResult()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            var tournaments = new List<Tournament>
            {
                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = DateTime.Now.AddMonths(-1) }
            };

            mockTournamentService
                .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync(tournaments);

            mockLeaderboardService
                .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                .ReturnsAsync(new Leaderboard
                {
                    Tournament = tournaments.First(),
                    Players = new List<PlayerResult> { new PlayerResult { Name = "Scottie Scheffler" } }
                });

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetLeaderboards_NoTournaments_ReturnsEmptyCollection()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            mockTournamentService
                .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync(Enumerable.Empty<Tournament>());

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task DatesShouldNotBeInFuture()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            var now = DateTime.Now;
            var tournaments = new List<Tournament>
            {
                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = now.AddMonths(-1) },
                new Tournament { TournamentID = 2, Name = "Tournament 2", StartDate = now.AddMonths(1) }
            };

            mockTournamentService
                .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync(tournaments);

            mockLeaderboardService
                .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                .ReturnsAsync((string id) => new Leaderboard
                {
                    Tournament = tournaments.First(t => t.TournamentID.ToString() == id),
                    Players = new List<PlayerResult> { new PlayerResult { Name = "Scottie Scheffler" } }
                });

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.False(result.Any(leaderboard => leaderboard.Tournament.StartDate > now), "Tournament date found in future");
        }

        [Fact]
        public async Task GetLeaderboards_ReturnsCorrectNumberOfTournaments()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            var tournaments = new List<Tournament>
            {
                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = DateTime.Now.AddMonths(-1) },
                new Tournament { TournamentID = 2, Name = "Tournament 2", StartDate = DateTime.Now.AddMonths(-2) }
            };

            mockTournamentService
                .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync(tournaments);

            mockLeaderboardService
                .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                .ReturnsAsync((string id) => new Leaderboard
                {
                    Tournament = tournaments.First(t => t.TournamentID.ToString() == id),
                    Players = new List<PlayerResult> { new PlayerResult { Name = "Scottie Scheffler", Rank = 1, TotalScore = 0 } }
                });

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetLeaderboards_ContainsSpecificTournamentName()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            var tournaments = new List<Tournament>
            {
                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = DateTime.Now.AddMonths(-1) }
            };

            mockTournamentService
                .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync(tournaments);

            mockLeaderboardService
                .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                .ReturnsAsync(new Leaderboard
                {
                    Tournament = tournaments.First(),
                    Players = new List<PlayerResult> { new PlayerResult { Name = "Scottie Scheffler", Rank = 1, TotalScore = 0 } }
                });

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.Contains(result, leaderboard => leaderboard.Tournament.Name == "Tournament 1");
        }

        [Fact]
        public async Task GetLeaderboards_TournamentsAreNull_ReturnsEmptyCollection()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            mockTournamentService
                .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync((IEnumerable<Tournament>?)null);

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ValidateTournaments_InvalidTournaments_ReturnsEmptyCollection()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            var tournaments = new List<Tournament>
            {
                new Tournament { TournamentID = 1, StartDate = DateTime.Now.AddMonths(-1) }
            };

            mockTournamentService.Setup(sut => sut.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync(tournaments);

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterPlayedLeaderboards_InvalidLeaderboards_ReturnsEmptyCollection()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            var tournaments = new List<Tournament>
            {
                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = DateTime.Now.AddMonths(-1) }
            };

            var leaderboards = new List<Leaderboard>
            {
                new Leaderboard
                {
                    Tournament = tournaments.First(),
                    Players = new List<PlayerResult> { new PlayerResult { Name = "Scottie Scheffler" } }
                }
            };

            mockTournamentService
                .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync(tournaments);

            mockLeaderboardService
                .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                .ReturnsAsync(leaderboards.First());

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterPlayedLeaderboards_LeaderboardsWithNoPlayers_ReturnsEmptyCollection()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            var tournaments = new List<Tournament>
            {
                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = DateTime.Now.AddMonths(-1) }
            };

            var leaderboards = new List<Leaderboard>
            {
                new Leaderboard
                {
                    Tournament = tournaments.First(),
                    Players = new List<PlayerResult> { }
                }
            };

            mockTournamentService
                .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync(tournaments);

            mockLeaderboardService
                .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                .ReturnsAsync(leaderboards.First());

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterPlayedLeaderboards_LeaderboardsWithoutScottieScheffler()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            var tournaments = new List<Tournament>
            {
                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = DateTime.Now.AddMonths(-1) }
            };

            var leaderboards = new List<Leaderboard>
            {
                new Leaderboard
                {
                    Tournament = tournaments.First(),
                    Players = new List<PlayerResult> { new PlayerResult { Name = "Other Player", Rank = 1, TotalScore = 0 } }
                }
            };

            mockTournamentService
                .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync(tournaments);

            mockLeaderboardService
                .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                .ReturnsAsync(leaderboards.First());

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterPlayedLeaderboards_LeaderboardsWithNoScottieScheffler()
        {
            // Arrange
            var mockLeaderboardService = new Mock<ILeaderboardService>();
            var mockTournamentService = new Mock<ITournamentService>();
            var tournamentValidator = new TournamentValidator();
            var leaderboardValidator = new LeaderboardValidator();

            var tournaments = new List<Tournament>
            {
                new Tournament { TournamentID = 1, Name = "Tournament 1", StartDate = DateTime.Now.AddMonths(-1) }
            };

            var leaderboards = new List<Leaderboard>
            {
                new Leaderboard
                {
                    Tournament = tournaments.First(),
                    Players = new List<PlayerResult> { }
                }
            };

            mockTournamentService
                .Setup(service => service.GetMostRecentTournaments(It.IsAny<int>()))
                .ReturnsAsync(tournaments);

            mockLeaderboardService
                .Setup(service => service.GetLeaderboard(It.IsAny<string>()))
                .ReturnsAsync(leaderboards.First());

            var playerTrackerService = new PlayerTrackerService(mockLeaderboardService.Object, mockTournamentService.Object, tournamentValidator, leaderboardValidator);

            // Act
            var result = await playerTrackerService.GetLeaderboards();

            // Assert
            Assert.Empty(result);
        }


    }
}
