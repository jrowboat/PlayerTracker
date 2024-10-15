using FluentValidation;
using PlayerTracker.Contracts;

namespace PlayerTracker.Services.Validators
{
    public class PlayerResultValidator : AbstractValidator<PlayerResult>
    {
        public PlayerResultValidator()
        {
            RuleFor(playerResult => playerResult.Name)
                .NotEmpty().WithMessage("Player name is required.");

            RuleFor(playerResult => playerResult.Rank)
                .NotEmpty().WithMessage("Player rank is required.")
                .GreaterThan(0).WithMessage("Player rank must be greater than 0.");

            RuleFor(playerResult => playerResult.TotalScore)
                .NotEmpty().WithMessage("Player total score is required.");
        }
    }
}
