using FluentValidation;

namespace PortfolioManagement.Api.Features.Instruments.SearchInstruments;

public sealed class SearchInstrumentsValidator : AbstractValidator<SearchInstrumentsRequest>
{
    public SearchInstrumentsValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Query is required.")
            .MinimumLength(2)
            .WithMessage("Query must be at least 2 characters long.");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .When(x => x.Limit.HasValue)
            .WithMessage("Limit must be between 1 and 100.");
    }
}
