using PortfolioManagement.Api.Features.Trades.AddTrade;
using PortfolioManagement.Api.Features.Trades.EditTrade;

namespace PortfolioManagement.Api.Domain.Tests;

public class TradeRequestContractTests
{
    private static readonly DateOnly ExecutedDate = new(2026, 1, 2);

    [Theory]
    [InlineData("Buy", 10)]
    [InlineData("buy", 10)]
    [InlineData("Sell", -10)]
    [InlineData("sell", -10)]
    public void AddTradeRequest_converts_type_and_shares_to_signed_quantity(string type, int expectedQuantity)
    {
        var request = new AddTradeRequest(1, type, 10, 100m, ExecutedDate);

        Assert.Equal(expectedQuantity, request.ToSignedQuantity());
    }

    [Theory]
    [InlineData("Buy", 10)]
    [InlineData("Sell", -10)]
    public void EditTradeRequest_converts_type_and_shares_to_signed_quantity(string type, int expectedQuantity)
    {
        var request = new EditTradeRequest(type, 10, 100m, ExecutedDate);

        Assert.Equal(expectedQuantity, request.ToSignedQuantity());
    }

    [Fact]
    public void AddTradeRequest_still_accepts_legacy_signed_quantity()
    {
        var request = new AddTradeRequest(1, null, null, 100m, ExecutedDate, -10);
        var result = new AddTradeValidator().Validate(request);

        Assert.True(result.IsValid);
        Assert.Equal(-10, request.ToSignedQuantity());
    }

    [Fact]
    public void EditTradeRequest_still_accepts_legacy_signed_quantity()
    {
        var request = new EditTradeRequest(null, null, 100m, ExecutedDate, -10);
        var result = new EditTradeValidator().Validate(request);

        Assert.True(result.IsValid);
        Assert.Equal(-10, request.ToSignedQuantity());
    }

    [Theory]
    [InlineData("Buy", 10, true)]
    [InlineData("Sell", 10, true)]
    [InlineData("Hold", 10, false)]
    [InlineData("Buy", 0, false)]
    public void AddTradeValidator_requires_valid_type_and_positive_shares(string type, int shares, bool isValid)
    {
        var request = new AddTradeRequest(1, type, shares, 100m, ExecutedDate);
        var result = new AddTradeValidator().Validate(request);

        Assert.Equal(isValid, result.IsValid);
    }

    [Theory]
    [InlineData("Buy", 10, true)]
    [InlineData("Sell", 10, true)]
    [InlineData("Hold", 10, false)]
    [InlineData("Buy", 0, false)]
    public void EditTradeValidator_requires_valid_type_and_positive_shares(string type, int shares, bool isValid)
    {
        var request = new EditTradeRequest(type, shares, 100m, ExecutedDate);
        var result = new EditTradeValidator().Validate(request);

        Assert.Equal(isValid, result.IsValid);
    }

    [Fact]
    public void AddTradeValidator_rejects_future_date()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var request = new AddTradeRequest(1, "Buy", 10, 100m, futureDate);

        var result = new AddTradeValidator().Validate(request);

        Assert.False(result.IsValid);
    }
}
