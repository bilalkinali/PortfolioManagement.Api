namespace PortfolioManagement.Api.Domain;

public class Position
{
    protected Position() { }

    private readonly List<Trade> _trades = [];

    private Position(int instrumentId)
    {
        if (instrumentId <= 0)
        {
            throw new ArgumentException("Instrument id must be valid.", nameof(instrumentId));
        }

        InstrumentId = instrumentId;
    }

    public int Id { get; protected set; }

    public int Quantity => CalculateMetrics().Quantity;
    public decimal AverageCostBasis => CalculateMetrics().AverageCostBasis;
    public decimal RealizedPnL => CalculateMetrics().RealizedPnL;

    public string Status => Quantity switch
    {
        > 0 => "Long",
        < 0 => "Short",
        _ => "Closed"
    };

    public DateOnly OpenDate => HasTrades
        ? _trades.Min(t => t.ExecutedDate)
        : throw new InvalidOperationException("Position has no trades.");

    public DateOnly? CloseDate => Quantity == 0 && HasTrades
        ? _trades.Max(t => t.ExecutedDate)
        : null;

    public int PortfolioId { get; protected set; }
    public int InstrumentId { get; protected set; }
    public Instrument Instrument { get; protected set; } = null!;

    public IReadOnlyCollection<Trade> Trades => _trades;
    public bool HasTrades => _trades.Count > 0;

    public static Position Create(
        int instrumentId,
        int quantity,
        decimal price,
        DateOnly executedDate,
        out Trade trade)
    {
        var position = new Position(instrumentId);
        trade = position.AddTrade(quantity, price, executedDate);

        return position;
    }

    public Trade AddTrade(int quantity, decimal price, DateOnly executedDate)
    {
        var trade = Trade.Create(quantity, price, executedDate);

        _trades.Add(trade);

        return trade;
    }

    public void EditTrade(int tradeId, int quantity, decimal price, DateOnly executedDate)
    {
        var trade = GetTrade(tradeId);

        trade.Edit(quantity, price, executedDate);
    }

    public void DeleteTrade(int tradeId)
    {
        var trade = GetTrade(tradeId);

        _trades.Remove(trade);
    }

    private Trade GetTrade(int tradeId)
    {
        return _trades.FirstOrDefault(t => t.Id == tradeId)
            ?? throw new InvalidOperationException("Trade does not exist.");
    }

    private PositionMetrics CalculateMetrics()
    {
        var quantity = 0;
        var averageCostBasis = 0m;
        var realizedPnL = 0m;

        foreach (var trade in OrderedTrades())
        {
            if (quantity == 0)
            {
                quantity = trade.Quantity;
                averageCostBasis = trade.Price;
                continue;
            }

            var sameDirection =
                quantity > 0 && trade.Quantity > 0 ||
                quantity < 0 && trade.Quantity < 0;

            if (sameDirection)
            {
                var currentAbsQuantity = Math.Abs(quantity);
                var tradeAbsQuantity = Math.Abs(trade.Quantity);

                averageCostBasis =
                    ((currentAbsQuantity * averageCostBasis) + (tradeAbsQuantity * trade.Price))
                    / (currentAbsQuantity + tradeAbsQuantity);

                quantity += trade.Quantity;
                continue;
            }

            var closingQuantity = Math.Min(Math.Abs(quantity), Math.Abs(trade.Quantity));

            if (quantity > 0)
            {
                // Closing/reducing long by selling.
                realizedPnL += closingQuantity * (trade.Price - averageCostBasis);
            }
            else
            {
                // Closing/reducing short by buying.
                realizedPnL += closingQuantity * (averageCostBasis - trade.Price);
            }

            var previousQuantity = quantity;
            quantity += trade.Quantity;

            if (quantity == 0)
            {
                averageCostBasis = 0m;
            }
            else if (Math.Abs(trade.Quantity) > Math.Abs(previousQuantity))
            {
                // Trade crossed zero and opened the opposite direction.
                averageCostBasis = trade.Price;
            }
            // else: partial close, averageCostBasis stays unchanged
        }

        return new PositionMetrics(quantity, averageCostBasis, realizedPnL);
    }

    private IEnumerable<Trade> OrderedTrades()
    {
        return _trades
            .OrderBy(t => t.ExecutedDate)
            .ThenBy(t => t.Id);
    }

    private sealed record PositionMetrics(
        int Quantity,
        decimal AverageCostBasis,
        decimal RealizedPnL);
}