using FluentValidation;

namespace PortfolioManagement.Api.Features.StockHistory.GetStockHistory;

public sealed class GetStockHistoryValidator : AbstractValidator<GetStockHistoryRequest>
{
    private static readonly string[] ValidTimespans =
    [
        "minute",
        "hour",
        "day",
        "week",
        "month"
    ];

    public GetStockHistoryValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty()
            .WithMessage("Ticker is required.");

        RuleFor(x => x.From)
            .NotEmpty()
            .Must(BeValidDate)
            .WithMessage("'from' must be a valid date.");

        RuleFor(x => x.To)
            .NotEmpty()
            .Must(BeValidDate)
            .WithMessage("'to' must be a valid date.");

        RuleFor(x => x)
            .Must(HaveValidDateRange)
            .WithMessage("'from' must be less than or equal to 'to'.");

        RuleFor(x => x.Timespan)
            .Must(BeValidTimespan)
            .WithMessage("Invalid timespan. Allowed values: minute, hour, day, week, month.");
    }

    private static bool BeValidDate(string value)
        => DateOnly.TryParse(value, out _);

    private static bool HaveValidDateRange(GetStockHistoryRequest request)
    {
        if (!DateOnly.TryParse(request.From, out var fromDate))
        {
            return true;
        }

        if (!DateOnly.TryParse(request.To, out var toDate))
        {
            return true;
        }

        return fromDate <= toDate;
    }

    private static bool BeValidTimespan(string? timespan)
    {
        if (string.IsNullOrWhiteSpace(timespan))
        {
            return true;
        }

        return ValidTimespans.Contains(timespan.Trim().ToLowerInvariant());
    }
}