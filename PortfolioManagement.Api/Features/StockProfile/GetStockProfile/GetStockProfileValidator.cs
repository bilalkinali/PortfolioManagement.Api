using FluentValidation;

namespace PortfolioManagement.Api.Features.StockProfile.GetStockProfile;

public sealed class GetStockProfileValidator : AbstractValidator<GetStockProfileRequest>
{
    public GetStockProfileValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty()
            .WithMessage("Ticker is required.");
    }
}
