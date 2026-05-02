using PortfolioManagement.Api.Shared.Events;

namespace PortfolioManagement.Api.Features.Trades.AddTrade;

public sealed record TradeAddedEvent(int InstrumentId) : IDomainEvent;