using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Features.Trades.AddTrade;
using PortfolioManagement.Api.Infrastructure.Persistence;
using PortfolioManagement.Api.Shared.Events;

namespace PortfolioManagement.Api.Features.Instruments.TrackInstrument;

public sealed class TrackInstrumentWhenTradeAddedHandler
    : IDomainEventHandler<TradeAddedEvent>
{
    private readonly PortfolioDbContext _dbContext;

    public TrackInstrumentWhenTradeAddedHandler(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(TradeAddedEvent domainEvent, CancellationToken cancellationToken)
    {
        var instrument = await _dbContext.Instruments
            .FirstAsync(i => i.Id == domainEvent.InstrumentId, cancellationToken);

        if (instrument.IsTracked)
        {
            return;
        }

        instrument.MarkAsTracked();

        // Get latest price? If not tracked before, price may be stale
        // After tracking, background worker should update prices

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}