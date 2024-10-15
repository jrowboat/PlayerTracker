using FluentValidation;
using PlayerTracker.Contracts;

namespace PlayerTracker.Services.Validators
{
  public class TournamentValidator : AbstractValidator<Tournament>
  {
    public TournamentValidator()
    {
        RuleFor(tournament => tournament.Name)
            .NotEmpty().WithMessage("Tournament name is required.");

        RuleFor(tournament => tournament.StartDate)
            .NotEmpty().WithMessage("Tournament start date is required.");
    }
  }
}
