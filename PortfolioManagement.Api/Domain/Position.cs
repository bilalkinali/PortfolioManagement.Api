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

        EnsureTradeDoesNotCrossZero(trade);

        _trades.Add(trade);

        return trade;
    }

    public void EditTrade(int tradeId, int quantity, decimal price, DateOnly executedDate)
    {
        var trade = GetTrade(tradeId);

        var oldQuantity = trade.Quantity;
        var oldPrice = trade.Price;
        var oldExecutedDate = trade.ExecutedDate;

        trade.Edit(quantity, price, executedDate);

        try
        {
            EnsurePositionNeverCrossesZero();
        }
        catch
        {
            trade.Edit(oldQuantity, oldPrice, oldExecutedDate);
            throw;
        }
    }

    public void DeleteTrade(int tradeId)
    {
        var trade = GetTrade(tradeId);

        _trades.Remove(trade);

        try
        {
            EnsurePositionNeverCrossesZero();
        }
        catch
        {
            _trades.Add(trade);
            throw;
        }
    }

    private Trade GetTrade(int tradeId)
    {
        return _trades.FirstOrDefault(t => t.Id == tradeId)
            ?? throw new InvalidOperationException("Trade does not exist.");
    }

    private void EnsureTradeDoesNotCrossZero(Trade trade)
    {
        if (!HasTrades)
        {
            return;
        }

        var currentQuantity = Quantity;
        var newQuantity = currentQuantity + trade.Quantity;

        if (currentQuantity > 0 && newQuantity < 0)
        {
            throw new InvalidOperationException("Trade cannot switch position directly from long to short. Close the long position first.");
        }

        if (currentQuantity < 0 && newQuantity > 0)
        {
            throw new InvalidOperationException("Trade cannot switch position directly from short to long. Close the short position first.");
        }
    }

    private void EnsurePositionNeverCrossesZero()
    {
        var quantity = 0;

        foreach (var trade in OrderedTrades())
        {
            var newQuantity = quantity + trade.Quantity;

            if (quantity > 0 && newQuantity < 0)
            {
                throw new InvalidOperationException("Position history cannot cross directly from long to short.");
            }

            if (quantity < 0 && newQuantity > 0)
            {
                throw new InvalidOperationException("Position history cannot cross directly from short to long.");
            }

            quantity = newQuantity;
        }
    }

    private PositionMetrics CalculateMetrics()
    {
        var quantity = 0;
        var averageEntryPrice = 0m;
        var realizedPnL = 0m;

        foreach (var trade in OrderedTrades())
        {
            if (quantity == 0)
            {
                quantity = trade.Quantity;
                averageEntryPrice = trade.Price;
                continue;
            }

            var sameDirection =
                quantity > 0 && trade.Quantity > 0 ||
                quantity < 0 && trade.Quantity < 0;

            if (sameDirection)
            {
                var currentAbsQuantity = Math.Abs(quantity);
                var tradeAbsQuantity = Math.Abs(trade.Quantity);

                averageEntryPrice =
                    ((currentAbsQuantity * averageEntryPrice) + (tradeAbsQuantity * trade.Price))
                    / (currentAbsQuantity + tradeAbsQuantity);

                quantity += trade.Quantity;
                continue;
            }

            var closingQuantity = Math.Abs(trade.Quantity);

            if (quantity > 0)
            {
                // Long position closed/reduced by selling.
                realizedPnL += closingQuantity * (trade.Price - averageEntryPrice);
            }
            else
            {
                // Short position closed/reduced by buying.
                realizedPnL += closingQuantity * (averageEntryPrice - trade.Price);
            }

            quantity += trade.Quantity;

            if (quantity == 0)
            {
                averageEntryPrice = 0m;
            }
        }

        return new PositionMetrics(quantity, averageEntryPrice, realizedPnL);
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