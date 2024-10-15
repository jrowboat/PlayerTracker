using FluentValidation.TestHelper;
using PlayerTracker.Contracts;
using PlayerTracker.Services.Validators;
using Xunit;

namespace PlayerTracker.UnitTests
{
    public class LeaderboardValidatorTests
    {
        [Fact]
        public void Validate_LeaderboardTournamentIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new LeaderboardValidator();
            var leaderboard = new Leaderboard { Tournament = null };

            // Act
            var result = validator.TestValidate(leaderboard);

            // Assert
            result.ShouldHaveValidationErrorFor(l => l.Tournament)
                .WithErrorMessage("Tournament is required.");
        }

        [Fact]
        public void Validate_LeaderboardPlayersIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new LeaderboardValidator();
            var leaderboard = new Leaderboard { Players = null };

            // Act
            var result = validator.TestValidate(leaderboard);

            // Assert
            result.ShouldHaveValidationErrorFor(l => l.Players)
                .WithErrorMessage("Players are required.");
        }

        [Fact]
        public void Validate_LeaderboardPlayersIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new LeaderboardValidator();
            var leaderboard = new Leaderboard { Players = new List<PlayerResult>() };

            // Act
            var result = validator.TestValidate(leaderboard);

            // Assert
            result.ShouldHaveValidationErrorFor(l => l.Players)
                .WithErrorMessage("At least one player is required.");
        }

        [Fact]
        public void Validate_LeaderboardDateInFuture_ShouldHaveValidationError()
        {
            // Arrange
            var validator = new LeaderboardValidator();
            var leaderboard = new Leaderboard
            {
                Tournament = new Tournament
                {
                    StartDate = DateTime.Now.AddDays(1)
                }
            };

            // Act
            var result = validator.TestValidate(leaderboard);

            // Assert
            result.ShouldHaveValidationErrorFor(l => l.Tournament.StartDate)
                .WithErrorMessage("Tournament start date must be in the past.");
        }
    }
}