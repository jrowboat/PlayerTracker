using FluentValidation;
using PlayerTracker.Contracts;

namespace PlayerTracker.Services.Validators
{
    public class LeaderboardValidator : AbstractValidator<Leaderboard>
    {
        public LeaderboardValidator()
        {
            RuleFor(leaderboard => leaderboard.Tournament)
                .NotNull().WithMessage("Tournament is required.")
                .SetValidator(new TournamentValidator());

            RuleFor(leaderboard => leaderboard.Players)
                .NotNull().WithMessage("Players are required.")
                .Must(players => players != null && players.Count > 0).WithMessage("At least one player is required.");

            RuleForEach(leaderboard => leaderboard.Players)
                .SetValidator(new PlayerResultValidator());
        }
    }
}
