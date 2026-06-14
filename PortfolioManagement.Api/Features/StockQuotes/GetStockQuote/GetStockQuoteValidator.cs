using FluentValidation;

namespace PortfolioManagement.Api.Features.StockQuotes.GetStockQuote;

public sealed class GetStockQuoteValidator : AbstractValidator<GetStockQuoteRequest>
{
    public GetStockQuoteValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty()
            .WithMessage("Ticker is required.")
            .MaximumLength(32)
            .WithMessage("Ticker must be 32 characters or fewer.");
    }
}
