using FluentValidation;

namespace PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;

public class CreatePortfolioValidator : AbstractValidator<CreatePortfolioRequest>
{
    public CreatePortfolioValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("'{PropertyName}' must not be empty.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
